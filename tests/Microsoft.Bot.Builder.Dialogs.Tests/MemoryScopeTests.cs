﻿#pragma warning disable SA1402 // File may only contain a single type
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.TriggerHandlers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder.LanguageGeneration.Templates;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class MemoryScopeTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void SimpleMemoryScopesTest()
        {
            var dc = new DialogContext(new DialogSet(), new TurnContext(new TestAdapter(), new Schema.Activity()), (DialogState)new DialogState());
            var dsm = new DialogStateManager(dc);

            foreach (var memoryScope in DialogStateManager.MemoryScopes.Where(ms => !(ms is ThisMemoryScope || ms is DialogMemoryScope)))
            {
                var memory = memoryScope.GetMemory(dc);
                Assert.IsNotNull(memory, "should get memory without any set");
                ObjectPath.SetValue(memory, "test", 15);
                memory = memoryScope.GetMemory(dc);
                Assert.AreEqual(15, ObjectPath.GetValue<int>(memory, "test"), "Should roundtrip memory");
                ObjectPath.SetValue(memory, "test", 25);
                memory = memoryScope.GetMemory(dc);
                Assert.AreEqual(25, ObjectPath.GetValue<int>(memory, "test"), "Should roundtrip memory2");
            }
        }

        [TestMethod]
        public async Task DialogMemoryScopeTest()
        {
            var storage = new MemoryStorage();
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .UseStorage(storage)
                .UseState(new UserState(storage), new ConversationState(storage))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            DialogManager dm = new DialogManager(new MemoryScopeTestDialog());

            await new TestFlow((TestAdapter)adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext);
            })
            .Send("hello")
                .AssertReply("next")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task TestPathResolvers()
        {
            var storage = new MemoryStorage();
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .UseStorage(storage)
                .UseState(new UserState(storage), new ConversationState(storage))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            DialogManager dm = new DialogManager(new PathResolverTestDialog());

            await new TestFlow((TestAdapter)adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext);
            })
            .Send("hello")
                .AssertReply("next")
            .StartTestAsync();
        }
    }

    internal class MemoryScopeTestDialog : Dialog
    {
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var scope in DialogStateManager.MemoryScopes.Where(ms => !(ms is DialogMemoryScope)).Select(ms => ms.Name))
            {
                var path = $"{scope}.test";
                Assert.IsNull(dc.State.GetValue<string>(path), $"{path} should be null");
                dc.State.SetValue(path, scope);
                Assert.IsNotNull(dc.State.GetValue<string>(path), $"{path} should not be null");
                Assert.AreEqual(scope, dc.State.GetValue<string>(path), $"{path} should be {scope}");
            }

            await dc.Context.SendActivityAsync("next");
            return await dc.EndDialogAsync();
        }
    }

    internal class PathResolverTestDialog : Dialog
    {
        private string[] entities = new string[] { "test1", "test2" };

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateSetValue(dc, "#test", "turn.recognized.intents.test");
            ValidateSetValue(dc, "$test", "dialog.test");
            ValidateSetValue(dc, "@test", "turn.recognized.entities.test[0]", entities);
            dc.State.RemoveValue("turn.recognized.entities");
            ValidateSetValue(dc, "@@test", "turn.recognized.entities.test", entities);
            Assert.AreEqual("test1", dc.State.GetValue<string>("@test"));
            Assert.AreEqual("test2", dc.State.GetValue<string[]>("@@test")[1]);

            ValidateRemoveValue(dc, "#test", "turn.recognized.intents.test");
            ValidateRemoveValue(dc, "$test", "dialog.test");
            ValidateValue(dc, "@test", "turn.recognized.entities.test[0]");
            ValidateRemoveValue(dc, "@@test", "turn.recognized.entities.test");

            await dc.Context.SendActivityAsync("next");
            return await dc.EndDialogAsync();
        }
        
        private void ValidateSetValue(DialogContext dc, string alias, string path, object value = null)
        {
            Assert.IsNull(dc.State.GetValue<object>(alias), $"{alias} should be null");
            dc.State.SetValue(path, value ?? alias);
            ValidateValue(dc, alias, path);
        }

        private void ValidateValue(DialogContext dc, string alias, string path)
        {
            Assert.IsNotNull(dc.State.GetValue<object>(path));
            Assert.IsNotNull(dc.State.GetValue<object>(alias));
            Assert.AreEqual(dc.State.GetValue<object>(alias), dc.State.GetValue<object>(path), $"{alias} should be same as {path}");
        }

        private void ValidateRemoveValue(DialogContext dc, string alias, string path)
        {
            ValidateValue(dc, alias, path);
            dc.State.RemoveValue(alias);
            Assert.IsNull(dc.State.GetValue<object>(path), $"property should be removed by alias {alias}");
            Assert.IsNull(dc.State.GetValue<object>(alias), $"property should be removed by alias {alias}");
        }
    }
}
