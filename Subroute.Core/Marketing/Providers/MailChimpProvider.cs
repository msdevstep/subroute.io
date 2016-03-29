using System;
using System.Collections.Generic;
using MailChimp;
using MailChimp.Helper;
using MailChimp.Lists;

namespace Subroute.Core.Marketing.Providers
{
    public interface IMailChimpProvider
    {
        void AddSubscriber(string firstName, string lastName, string email);
    }

    public class MailChimpProvider : IMailChimpProvider
    {
        public void AddSubscriber(string firstName, string lastName, string email)
        {
            try
            {
                var manager = new MailChimpManager(Settings.MailChimpApiKey);
                var list = new List<BatchEmailParameter>();

                var subscriber = new BatchEmailParameter()
                {
                    Email = new EmailParameter() { Email = email },
                    MergeVars = new MergeVar()
                    {
                        { "FNAME", firstName },
                        { "LNAME", lastName }
                    }
                };
                
                list.Add(subscriber);

                var result = manager.BatchSubscribe(Settings.MailChimpAllSubscriberListId, list, false);
            }
            catch (Exception)
            {
                // TODO: Log the exception
            }
        }
    }
}
