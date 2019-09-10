﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class DialogContextState : IDictionary<string, object>
    {
        /// <summary>
        /// Common state properties paths.
        /// </summary>
#pragma warning disable SA1310 // Field should not contain underscore.
        public const string DIALOG_OPTIONS = "dialog.options";
        public const string DIALOG_VALUE = "dialog.value";

        public const string TURN_ACTIVITY = "turn.activity";
        public const string TURN_RECOGNIZED = "turn.recognized";
        public const string TURN_TOPINTENT = "turn.recognized.intent";
        public const string TURN_TOPSCORE = "turn.recognized.score";
        public const string TURN_STEPCOUNT = "turn.stepCount";
        public const string TURN_DIALOGEVENT = "turn.dialogEvent";

        public const string STEP_OPTIONS_PROPERTY = "dialog.step.options";
#pragma warning restore SA1310 // Field should not contain underscore.

        private const string PrefixCallBack = "callstackScope('";

        private static JsonSerializerSettings expressionCaseSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            NullValueHandling = NullValueHandling.Ignore,
        };

        private readonly DialogContext dialogContext;

        public DialogContextState(DialogContext dc, IDictionary<string, object> settings, IDictionary<string, object> userState, IDictionary<string, object> conversationState, IDictionary<string, object> turnState)
        {
            this.dialogContext = dc ?? throw new ArgumentNullException(nameof(dc));
            this.Settings = settings;
            this.User = userState;
            this.Conversation = conversationState;
            this.Turn = turnState;
        }

        /// <summary>
        /// Gets or sets settings for the application.
        /// </summary>
        /// <value>
        /// Settings for the application.
        /// </value>
        [JsonProperty(PropertyName = "settings")]
        public IDictionary<string, object> Settings { get; set; }

        /// <summary>
        /// Gets or sets state associated with the active user in the turn.
        /// </summary>
        /// <value>
        /// State associated with the active user in the turn.
        /// </value>
        [JsonProperty(PropertyName = "user")]
        public IDictionary<string, object> User { get; set; }

        /// <summary>
        /// Gets or sets state assocaited with the active conversation for the turn.
        /// </summary>
        /// <value>
        /// State assocaited with the active conversation for the turn.
        /// </value>
        [JsonProperty(PropertyName = "conversation")]
        public IDictionary<string, object> Conversation { get; set; }

        /// <summary>
        /// Gets or sets state associated with the active dialog for the turn.
        /// </summary>
        /// <value>
        /// State associated with the active dialog for the turn.
        /// </value>
        [JsonProperty(PropertyName = "dialog")]
        public IDictionary<string, object> Dialog
        {
            get
            {
                var instance = dialogContext.ActiveDialog;

                if (instance == null)
                {
                    if (dialogContext.Parent != null)
                    {
                        instance = dialogContext.Parent.ActiveDialog;
                    }
                    else
                    {
                        throw new Exception("DialogContext.State.Dialog: no active or parent dialog instance.");
                    }
                }

                return instance.State;
            }

            set
            {
                var instance = dialogContext.ActiveDialog;

                if (instance == null)
                {
                    if (dialogContext.Parent != null)
                    {
                        instance = dialogContext.Parent.ActiveDialog;
                    }
                    else
                    {
                        throw new Exception("DialogContext.State.Dialog: no active or parent dialog instance.");
                    }
                }

                instance.State = value;
            }
        }

        /// <summary>
        /// Gets access to the callstack of dialog state.
        /// </summary>
        /// <value>
        /// Access to the callstack of dialog state.
        /// </value>
        [JsonIgnore]
        public IEnumerable<object> CallStack
        {
            get
            {
                // get each state on the current stack.
                foreach (var instance in this.dialogContext.Stack)
                {
                    if (instance.State != null)
                    {
                        yield return instance.State;
                    }
                }

                // switch to parent stack and enumerate it's state objects
                if (this.dialogContext.Parent != null)
                {
                    foreach (var state in dialogContext.Parent.State.CallStack)
                    {
                        yield return state;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets state associated with the current turn only (this is non-persisted).
        /// </summary>
        /// <value>
        /// State associated with the current turn only (this is non-persisted).
        /// </value>
        [JsonProperty(PropertyName = "turn")]
        public IDictionary<string, object> Turn { get; set; }

        public ICollection<string> Keys => new[] { "user", "conversation", "dialog", "callstack", "turn", "settings" };

        public ICollection<object> Values => new object[] { User, Conversation, Dialog, CallStack, Turn };

        public int Count => 3;

        public bool IsReadOnly => true;

        public object this[string key]
        {
            get
            {
                if (TryGetValue(key, out object result))
                {
                    return result;
                }

                return null;
            }

            set
            {
                System.Diagnostics.Trace.TraceError("DialogContextState doesn't support adding/changinge the base properties");
            }
        }

        public DialogContextVisibleState ToJson()
        {
            var instance = dialogContext.ActiveDialog;

            if (instance == null)
            {
                if (dialogContext.Parent != null)
                {
                    instance = dialogContext.Parent.ActiveDialog;
                }
            }

            return new DialogContextVisibleState()
            {
                Conversation = this.Conversation,
                User = this.User,
                Dialog = (Dictionary<string, object>)instance?.State,
            };
        }

        public IEnumerable<JToken> Query(string pathExpression)
        {
            JToken json = JToken.FromObject(this);

            return json.SelectTokens(pathExpression);
        }

        // TODO drop this function after we move RemoveProperty to use expressions
        public string ResolvePathShortcut(string path)
        {
            path = path.Trim();
            if (path.Length == 0)
            {
                return path;
            }

            string name = path.Substring(1);

            switch (path[0])
            {
                case '$':
                    // $title == dialog.title
                    return $"dialog.{name}";

                default:
                    return path;
            }
        }

        public object GetValue(string pathExpression)
        {
            return ObjectPath.GetValue<object>(this, pathExpression);
        }

        public object GetValue(Expression pathExpression)
        {
            return ObjectPath.GetValue<object>(this, pathExpression);
        }

        public object GetValue(string pathExpression, object defaultValue)
        {
            return ObjectPath.GetValue<object>(this, pathExpression, defaultValue);
        }

        public object GetValue(Expression pathExpression, object defaultValue)
        {
            return ObjectPath.GetValue<object>(this, pathExpression, defaultValue);
        }

        public T GetValue<T>(string pathExpression)
        {
            return ObjectPath.GetValue<T>(this, pathExpression);
        }

        public T GetValue<T>(Expression pathExpression)
        {
            return ObjectPath.GetValue<T>(this, pathExpression);
        }

        public T GetValue<T>(string pathExpression, T defaultVal)
        {
            if (ObjectPath.TryGetValue<T>(this, pathExpression, out var val))
            {
                return val;
            }

            return defaultVal;
        }

        public T GetValue<T>(Expression pathExpression, T defaultVal)
        {
            if (ObjectPath.TryGetValue<T>(this, pathExpression, out var val))
            {
                return val;
            }

            return defaultVal;
        }

        public bool TryGetValue<T>(string pathExpression, out T val)
        {
            return ObjectPath.TryGetValue(this, pathExpression, out val);
        }

        public bool TryGetValue<T>(Expression pathExpression, out T val)
        {
            return ObjectPath.TryGetValue(this, pathExpression, out val);
        }

        public bool HasValue(string pathExpression)
        {
            return ObjectPath.HasValue(this, pathExpression);
        }

        public bool HasValue(Expression pathExpression)
        {
            return ObjectPath.HasValue(this, pathExpression);
        }

        public void SetValue(string pathExpression, object value)
        {
            SetValue(new ExpressionEngine().Parse(pathExpression), value);
        }

        public void SetValue(Expression pathExpression, object value)
        {
            if (value is Task)
            {
                throw new Exception($"{pathExpression} = You can't pass an unresolved Task to SetValue");
            }

            var e = pathExpression.ToString();
            if (e.StartsWith(PrefixCallBack))
            {
                // turn $foo which comes in as callbackStack('foo') => dialog.foo
                pathExpression = new ExpressionEngine().Parse($"dialog.{e.Substring(PrefixCallBack.Length, e.Length - PrefixCallBack.Length - 2)}");
            }

            ObjectPath.SetValue(this, pathExpression, value);
        }

        public void RemoveProperty(string pathExpression)
        {
            // TODO move to ObjectPath and use Expressions properly
            ObjectPath.RemoveProperty(this, ResolvePathShortcut(pathExpression));
        }

        public void RemoveProperty(Expression pathExpression)
        {
            // TODO move to ObjectPath and use Expressions properly
            this.RemoveProperty(pathExpression.ToString());
        }

        public void Add(string key, object value)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            return this.Keys.Contains(key.ToLower());
        }

        public bool Remove(string pathExpression)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out object value)
        {
            value = null;
            switch (key.ToLower())
            {
                case "user":
                    value = this.User;
                    return true;
                case "conversation":
                    value = this.Conversation;
                    return true;
                case "dialog":
                    value = this.Dialog;
                    return true;
                case "callstack":
                    value = this.CallStack;
                    return true;
                case "settings":
                    value = this.Settings;
                    return true;
                case "turn":
                    value = this.Turn;
                    return true;
            }

            return false;
        }

        public void Add(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            yield return new KeyValuePair<string, object>("user", this.User);
            yield return new KeyValuePair<string, object>("conversation", this.Conversation);
            yield return new KeyValuePair<string, object>("dialog", this.Dialog);
            yield return new KeyValuePair<string, object>("callstack", this.CallStack);
            yield return new KeyValuePair<string, object>("settings", this.Settings);
            yield return new KeyValuePair<string, object>("turn", this.Turn);
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
