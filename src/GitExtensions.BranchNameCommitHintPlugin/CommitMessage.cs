using GitExtensions.BranchNameCommitHintPlugin.Properties;
using System.Drawing;

namespace GitExtensions.BranchNameCommitHintPlugin
{
    class CommitMessage
    {
        public string Text;
        public Image Icon;

        public CommitMessage(string text)
        {
            Text = text;
            Icon = Resources.Branch;
        }

        public CommitMessage(string text, Image icon)
        {
            Text = text;
            Icon = icon;
        }
    }
}
