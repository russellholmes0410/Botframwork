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
    Generator = new TemplateEngineLanguageGenerator(),
                
    Triggers = new List<OnCondition>()
    {
        new OnBeginDialog() {
            Actions = new List<Dialog>() {
                new TextInput() {
                    Prompt = new ActivityTemplate("What is your name?"),
                    Property = "user.name",
                    AllowInterruptions = AllowInterruptions.Always,
                    MaxTurnCount = 3,
                    DefaultValue = "'Human'",
                    Validations = new List<string>()
                    {
                        "length(this.value) > 2",
                        "length(this.value) <= 300"
                    },
                    InvalidPrompt = new ActivityTemplate("Sorry, '{this.value}' does not work. Give me something between 2-300 character in length. What is your name?"),
                    DefaultValueResponse = new ActivityTemplate("Sorry, I'm not getting it. For now, let's set your name to '{this.options.DefaultValue}'.")
                }
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
