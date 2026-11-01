using System;
using System.Web.UI;

namespace CodeLab.Modules.Public
{
    // CodeLab does not send email, so each option opens the visitor's own email app.
    // Site.Master lets signed-in users open this page too (see OpenPages).
    public partial class Contact : Page
    {
        // The one place this address is set. Change it to a real inbox before going live.
        protected const string SupportEmail = "support@codelab.local";

        // A mailto link, with the subject filled in when there is one
        protected static string MailLink(string subject)
        {
            string link = "mailto:" + SupportEmail;
            return subject == null ? link : link + "?subject=" + Uri.EscapeDataString(subject);
        }
    }
}
