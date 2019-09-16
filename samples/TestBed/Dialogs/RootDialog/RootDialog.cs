﻿using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.TriggerHandlers;
using Microsoft.Bot.Builder.LanguageGeneration.Templates;
using Microsoft.Bot.Builder.LanguageGeneration.Generators;
using Microsoft.Bot.Builder.LanguageGeneration;
using System.IO;
    
namespace Microsoft.BotBuilderSamples
{
    public class RootDialog : ComponentDialog
    {
        public RootDialog()
            : base(nameof(RootDialog))
        {
            var lgFile = Path.Combine(".", "Dialogs", "RootDialog", "RootDialog.lg");

            // Create instance of adaptive dialog. 
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                
                Generator = new TemplateEngineLanguageGenerator(new TemplateEngine().AddFile(lgFile)),
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>() {
                        new IntentPattern() {
                            Intent = "Start",
                            Pattern = "(?i)start"
                        }

                    }
                },
                //AutoEndDialog = false,
                Triggers = new List<TriggerHandler>()
                {
                    new OnConversationUpdateActivity()
                    {
                        Constraint = "toLower(turn.Activity.membersAdded[0].name) != 'bot'",
                        Actions = WelcomeUserAction()
                    },
                    new OnIntent() {
                        Intent = "Start",
                        Actions = new List<Dialog>() {
                            new BeginDialog()
                            {
                                Id = "childDialog",
                                ResultProperty = "$result"
                            },
                            new SendActivity("In outer dialog: I have {join($result, ',')")
                        }
                    }
                }
            };

            var childDialog = new AdaptiveDialog("childDialog")
            {
                Triggers = new List<TriggerHandler>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new TextInput() {
                                Prompt = new ActivityTemplate("What is your name?"),
                                Property = "$name",
                                AllowInterruptions = AllowInterruptions.Never,
                                MaxTurnCount = 3,
                                DefaultValue = "'Human'",
                                DefaultValueResponse = new ActivityTemplate("Sorry, this is not working. For now, I've set your name as 'Human'"),
                                Validations = new List<string>()
                                {
                                    "length(turn.value) > 2",
                                    "length(turn.value) <= 300"
                                },
                                InvalidPrompt = new ActivityTemplate("Sorry, '{turn.value}' does not work. Give me something between 2-300 character in length. What is your name?")
                            },
                            new NumberInput()
                            {
                                Prompt = new ActivityTemplate("What is your age?"),
                                Property = "$age",
                                AllowInterruptions = AllowInterruptions.Never,
                                MaxTurnCount = 3,
                                DefaultValue = "30",
                                Validations = new List<string>()
                                {
                                    "int(turn.value) >= 1",
                                    "int(turn.value) <= 150"
                                },
                                InvalidPrompt = new ActivityTemplate("Sorry, '{turn.value}' does not work. Give me something between 1-150. What is your age?")
                            },
                            new EditArray()
                            {
                                ItemsProperty = "$result",
                                Value = "$name",
                                ChangeType = EditArray.ArrayChangeType.Push
                            },
                            new EditArray()
                            {
                                ItemsProperty = "$result",
                                Value = "$age",
                                ChangeType = EditArray.ArrayChangeType.Push
                            },
                            new SendActivity("I have {join($result, ',')"),
                            new EndDialog()
                            {
                                Value = "$result"
                            }
                        }
                    }
                }
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(rootDialog);
            AddDialog(childDialog);

            // The initial child Dialog to run.
            InitialDialogId = nameof(AdaptiveDialog);
        }
        private static List<Dialog> WelcomeUserAction()
        {
            return new List<Dialog>()
            {
                // Iterate through membersAdded list and greet user added to the conversation.
                new Foreach()
                {
                    ItemsProperty = "turn.activity.membersAdded",
                    Actions = new List<Dialog>()
                    {
                        // Note: Some channels send two conversation update events - one for the Bot added to the conversation and another for user.
                        // Filter cases where the bot itself is the recipient of the message. 
                        new IfCondition()
                        {
                            Condition = "dialog.foreach.value.name != turn.activity.recipient.name",
                            Actions = new List<Dialog>()
                            {
                                new SendActivity("[WelcomeUser]")
                            }
                        }
                    }
                }
            };

        }
    }
}
