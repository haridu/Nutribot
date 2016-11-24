﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

using System.Collections.Generic;
using Nutri_Bot;

using Microsoft.Bot.Builder.Dialogs;
using Weather_Bot.Models;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using Weather_Bot;
using Nutribot.models;
using System.Security.Cryptography;

namespace Nutri_Bot
{


    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {

            if (activity.Type == ActivityTypes.Message)
            {



                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                StateClient stateClient = activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);


                var userMessage = activity.Text;
                bool isadministartion = false;








                switch (userMessage.ToLower())
                {
                    case "clear":
                        await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                        Activity reply = activity.CreateReply($"User data is cleared");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                        break;
                    case "login":


                        break;
                    case "hello":
                    case "hey":
                    case "hi":
                        if (userData.GetProperty<bool>("SentGreeting"))
                        {
                            Activity helloagain = activity.CreateReply($"hello again " + userData.GetProperty<string>("Name") + " :)");
                            await connector.Conversations.ReplyToActivityAsync(helloagain);
                        }
                        else
                        {

                            var markdownContent1 = "# Hello :) \n";

                            markdownContent1 += "Hi! ,im Nutrio bot i can get nutrition information of food and save your food into favorites list as you wish,to use this app you have set your name first :)\n\n";


                            markdownContent1 += "![](https://cloud.githubusercontent.com/assets/7879247/20550513/456b0452-b19b-11e6-9dc8-91847505f169.png)\n";


                            Activity y = activity.CreateReply(markdownContent1);

                            await connector.Conversations.ReplyToActivityAsync(y);


                            userData.SetProperty<bool>("SentGreeting", true);
                            await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                        }
                        break;
                    default:


                        if (userMessage.Length < 8)
                        {
                            Activity passedhere = activity.CreateReply($"unknowen command");
                            await connector.Conversations.ReplyToActivityAsync(passedhere);
                            break;
                        }


                        if (userMessage.ToLower().Substring(0, 6).Equals("create"))
                        {
                            isadministartion = true;

                            String loginsting = userMessage.ToLower();
                            string[] splited = loginsting.Split(new char[0]);

                            string username = splited[1];
                            string password = splited[2];

                            string hashpass = password.GetHashCode().ToString();




                            // var md5 = new MD5CryptoServiceProvider();
                            // var md5data = md5.ComputeHash(data);

                            login login = new login()
                            {
                                username = username,
                                password = hashpass

                            };

                            List<login> logines = await AzureLoginManager.AzureManagerInstance.Getlogins();

                            bool userexist = false;
                            foreach (login l in logines)
                            {

                                if (l.username.Equals(username))
                                {
                                    Activity loginerr = activity.CreateReply($"error,username is taken try diffrent username");
                                    await connector.Conversations.ReplyToActivityAsync(loginerr);
                                    userexist = true;

                                }

                            }

                            if (userexist == false)
                            {
                                await AzureLoginManager.AzureManagerInstance.AddTimeline(login);

                                Activity newlogin = activity.CreateReply($"account created");
                                await connector.Conversations.ReplyToActivityAsync(newlogin);

                            }





                        }
                        if (userMessage.ToLower().Substring(0, 5).Equals("login"))
                        {
                            isadministartion = true;
                            //clears userdata
                            //await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);


                            String loginsting = userMessage.ToLower();
                            string[] splited = loginsting.Split(new char[0]);

                            string username = splited[1];
                            string password = splited[2];

                            string hashpass = password.GetHashCode().ToString();

                            List<login> logines = await AzureLoginManager.AzureManagerInstance.Getlogins();
                            bool userfound = false;

                            foreach (login l in logines)
                            {


                                if (l.username.Equals(username))
                                {
                                    userfound = true;
                                    if (l.password.Equals(hashpass))
                                    {
                                        Activity login = activity.CreateReply($"login was sucessfull");
                                        await connector.Conversations.ReplyToActivityAsync(login);

                                        userData.SetProperty<bool>("logged", true);
                                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);

                                        userData.SetProperty<string>("user", username);
                                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);


                                    }
                                    else
                                    {

                                        Activity login = activity.CreateReply($"wronge password");
                                        await connector.Conversations.ReplyToActivityAsync(login);

                                    }


                                }


                            }


                            if (!userfound)
                            {
                                Activity login = activity.CreateReply($"username was not found");
                                await connector.Conversations.ReplyToActivityAsync(login);
                            }


                        }

                        
                       


                        if (userMessage.ToLower().Substring(0, 8).Equals("set name"))
                        {
                            

                        }
                        else
                        {
                            bool logged = userData.GetProperty<bool>("logged");
                            if (!logged == true)
                            {
                                if (isadministartion == false)
                                {

                                    //endOutput = "Home City not assigned";
                                    Activity names = activity.CreateReply($"  you are not logged in ,you have to login to your to use this app, login by typing login");
                                    await connector.Conversations.ReplyToActivityAsync(names);
                                }

                            }
                            else
                            {
                                if (isadministartion == false)
                                {
                                    



                                    if (userMessage.ToLower().Substring(0, 6).Equals("delete"))
                                    {
                                        
                                        String importance = userMessage.ToLower();
                                        string[] splited = importance.Split(new char[0]);

                                        string id = splited[1];


                                        Activity nameassined = activity.CreateReply($" food id" + id + " was deleted from favorites");
                                        await connector.Conversations.ReplyToActivityAsync(nameassined);

                                        List<Timeline> timelines = await AzureManager.AzureManagerInstance.GetTimelines();
                                        bool iteamfound = false;

                                        foreach (Timeline t in timelines)
                                        {


                                            if (t.id.Equals("1"))
                                            {
                                                iteamfound = true;
                                                await AzureManager.AzureManagerInstance.DeleteTimeline(t);

                                               

                                           }

                                        }

                                        if (iteamfound == true)
                                        {
                                            Activity name = activity.CreateReply($" food id" + id + " was deleted from favorites");
                                            await connector.Conversations.ReplyToActivityAsync(name);
                                        }
                                        else
                                        {
                                            Activity name = activity.CreateReply($" food id" + id + " was not found");
                                            await connector.Conversations.ReplyToActivityAsync(name);
                                        }

                                    }
                                   


                                    if (userMessage.ToLower().Substring(0, 14).Equals("set importance"))
                                    {
                                        String importance = userMessage.ToLower();
                                        string[] splited = importance.Split(new char[0]);
                                        List<Timeline> timelines = await AzureManager.AzureManagerInstance.GetTimelines();
                                        string id = splited[2];
                                        string value = splited[3];

                                        foreach (Timeline t in timelines)
                                        {

                                            if (t.id.Equals(id.ToLower()))
                                            {

                                                t.importance = value;
                                                await AzureManager.AzureManagerInstance.UpdateTimeline(t);
                                                Activity update = activity.CreateReply($" importance of "+t.food+" was updated");
                                                await connector.Conversations.ReplyToActivityAsync(update);
                                            }

                                        }

                                       

                                    }




                                    await Conversation.SendAsync(activity, () => new Luis(userData.GetProperty<string>("user")));
                                }
                            }

                        }
                        break;


                }


            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}