using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.LanguageGeneration.Templates;
using Microsoft.Bot.Builder.LanguageGeneration.Generators;
using Microsoft.Bot.Builder.LanguageGeneration;
using System.IO;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;

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
                        new IntentPattern()
                        {
                            Intent = "NoName",
                            Pattern = "(?i)no"
                        }
                    }
                },
                //AutoEndDialog = false,
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new TextInput() {
                                Prompt = new ActivityTemplate("What is your name?"),
                                Property = "$userName"
                            },
                            new SendActivity("I have {$userName} as your name")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "NoName",
                        Actions = new List<Dialog>()
                        {
                            new SetProperty()
                            {
                                Property = "$userName",
                                Value = "'Human'"
                            }
                            ,new SendActivity("I'm going with {$userName}")
                        }
                    }
                }
            };


            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(rootDialog);

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
