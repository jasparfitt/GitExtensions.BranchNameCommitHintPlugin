using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using ResourceManager;
using GitExtensions.BranchNameCommitHintPlugin.Properties;
using System.Text.RegularExpressions;
using System.Drawing;

namespace GitExtensions.BranchNameCommitHintPlugin
{
    [Export(typeof(IGitPlugin))]
    public class Plugin : GitPluginBase, IGitPluginForRepository
    {
        private const string DefaultFormat = "{0} your message";
        private const string DefaultRegex = @"(([a-z]|[A-Z]|[0-9])+-([0-9])+)\w";

        private readonly BoolSetting EnabledSetting = new BoolSetting("Plugin enabled", true);
        private readonly PseudoSetting RegexInfo = new PseudoSetting("The regex used to parse the key from your branch name");
        private readonly StringSetting RegexStringSetting = new StringSetting("Regex", "Regex", DefaultRegex, true);
        private readonly PseudoSetting PlainInfo = new PseudoSetting("Enables a commit template with only the branch key");
        private readonly BoolSetting PlainEnabledSetting = new BoolSetting("Plain branch key", false);
        private readonly PseudoSetting SemanticInfo1 = new PseudoSetting("Enables a set of commit templates with the branch key followed by a common semantic types.");
        private readonly PseudoSetting SemanticInfo2 = new PseudoSetting("For example \"ABC-123 feat: \"");
        private readonly BoolSetting SemanticEnabledSetting = new BoolSetting("Semantic branch key", true);
        private readonly PseudoSetting CustomInfo = new PseudoSetting("Enables the custom commit templates defined below, the branch key gets substituted for {0}");
        private readonly BoolSetting CustomEnabledSetting = new BoolSetting("Custom branch key", false);
        private readonly List<StringSetting> CustomTemplateSettings;

        private List<string> CustomTemplates = new List<string>();
        private List<CommitMessage> CurrentMessages;
        private string CurrentRegex = DefaultRegex;

        public Plugin() : base(true)
        {
            SetNameAndDescription("Branch Commit Hint");
            Translate();
            Icon = Resources.Branch;
            CustomTemplateSettings = new List<StringSetting>()
            {
                new StringSetting("Message Template 1", "Message Template", DefaultFormat, true),
                new StringSetting("Message Template 2", "Message Template", ""),
                new StringSetting("Message Template 3", "Message Template", ""),
                new StringSetting("Message Template 4", "Message Template", ""),
                new StringSetting("Message Template 5", "Message Template", "")
            };

            foreach (StringSetting setting in CustomTemplateSettings)
            {
                setting.CustomControl = new TextBox()
                {
                    Height = 56,
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical
                };
            }
        }

        public override bool Execute(GitUIEventArgs args)
        {
            args.GitUICommands.StartSettingsDialog(this);

            return false;
        }

        public override void Register(IGitUICommands gitUiCommands)
        {
            base.Register(gitUiCommands);
            gitUiCommands.PostSettings += GitUiCommandsOnPostSettings;
            gitUiCommands.PreCommit += GitUiCommandsOnPreCommit;
            gitUiCommands.PostCommit += GitUiCommandsOnPostCommit;
            gitUiCommands.PostRepositoryChanged += GitUiCommandsOnPostCommit;

            UpdateSettings();
        }

        public override IEnumerable<ISetting> GetSettings()
        {
            PseudoSetting resetButtonSetting = new PseudoSetting("Reset button", "");
            Button resetButton = new Button()
            {
                Text = "Reset",
                Height = 30
            };
            resetButton.Click += ResetRegex;
            resetButtonSetting.CustomControl = resetButton;
            int length = 4;
            PseudoSetting[] bars = new PseudoSetting[length];

            for (int i = 0; i < length; i++)
            {
                Control background = new Control() { BackColor = Color.Black };
                PseudoSetting bar = new PseudoSetting(null, null, 1)
                {
                    CustomControl = background
                };
                bars[i] = bar;
            }

            yield return EnabledSetting;
            yield return bars[0];
            yield return RegexInfo;
            yield return RegexStringSetting;
            yield return resetButtonSetting;
            yield return bars[1];
            yield return PlainInfo;
            yield return PlainEnabledSetting;
            yield return bars[2];
            yield return SemanticInfo1;
            yield return SemanticInfo2;
            yield return SemanticEnabledSetting;
            yield return bars[3];
            yield return CustomInfo;
            yield return CustomEnabledSetting;

            foreach (StringSetting setting in CustomTemplateSettings)
            {
                yield return setting;
            }
        }

        private void ResetRegex(object sender, System.EventArgs e)
        {
            RegexStringSetting.CustomControl.Text = DefaultRegex;
        }

        public override void Unregister(IGitUICommands gitUiCommands)
        {
            base.Unregister(gitUiCommands);
            gitUiCommands.PreCommit -= GitUiCommandsOnPreCommit;
            gitUiCommands.PostSettings -= GitUiCommandsOnPostSettings;
            gitUiCommands.PostCommit -= GitUiCommandsOnPostCommit;
            gitUiCommands.PostRepositoryChanged -= GitUiCommandsOnPostCommit;
        }

        private void UpdateSettings()
        {
            if (!EnabledSetting.ValueOrDefault(Settings))
            {
                return;
            }

            CurrentRegex = RegexStringSetting.ValueOrDefault(Settings);

            CustomTemplates = new List<string>();
            foreach (StringSetting setting in CustomTemplateSettings)
            {
                CustomTemplates.Add(setting.ValueOrDefault(Settings));
            }
        }

        private void GitUiCommandsOnPreCommit(object sender, GitUIEventArgs e)
        {
            if (!EnabledSetting.ValueOrDefault(Settings))
            {
                return;
            }

            CurrentMessages = new List<CommitMessage>();

            string branchId = "unknown";
            string branchName = e.GitModule.GetSelectedBranch();
            Regex rx = new Regex(CurrentRegex);
            Match match = rx.Match(branchName);

            if (match.Value != "")
            {
                branchId = match.Value;
            }

            if (PlainEnabledSetting.ValueOrDefault(Settings))
            {
                CurrentMessages.Add(new CommitMessage(branchId));
            }

            if (SemanticEnabledSetting.ValueOrDefault(Settings))
            {
                CurrentMessages.Add(new CommitMessage(branchId + " feat: ", Resources.Feat));
                CurrentMessages.Add(new CommitMessage(branchId + " fix: ", Resources.Fix));
                CurrentMessages.Add(new CommitMessage(branchId + " docs: ", Resources.Docs));
                CurrentMessages.Add(new CommitMessage(branchId + " style: ", Resources.Style));
                CurrentMessages.Add(new CommitMessage(branchId + " refactor: ", Resources.Refactor));
                CurrentMessages.Add(new CommitMessage(branchId + " test: ", Resources.Test));
                CurrentMessages.Add(new CommitMessage(branchId + " WIP: ", Resources.WIP));
            }

            if (CustomEnabledSetting.ValueOrDefault(Settings))
            {
                foreach (string template in CustomTemplates)
                {
                    if (template != "")
                    {
                        CurrentMessages.Add(new CommitMessage(string.Format(template, branchId)));
                    }
                }
            }

            foreach (CommitMessage message in CurrentMessages)
            {
                e.GitUICommands.AddCommitTemplate(message.Text, () => message.Text, message.Icon);
            }
        }

        private void GitUiCommandsOnPostCommit(object sender, GitUIEventArgs e)
        {
            if (!EnabledSetting.ValueOrDefault(Settings))
            {
                return;
            }

            if (CurrentMessages != null)
            {
                foreach (CommitMessage message in CurrentMessages)
                {
                    e.GitUICommands.RemoveCommitTemplate(message.Text);
                }
            }
        }

        private void GitUiCommandsOnPostSettings(object sender, GitUIPostActionEventArgs e)
        {
            UpdateSettings();
        }
    }
}
