using OhIlSeokBot.KakaoPlusFriend.Models;
using Microsoft.Bot.Connector.DirectLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OhIlSeokBot.KakaoPlusFriend.Helpers
{
    public static class MessageConvertor
    {
        public static Models.MessageResponse DirectLineToKakao(IList<Activity> activities)
        {
            if (activities == null || activities.Count <= 0) return null;

            var msg = new Models.MessageResponse();
            // 여러개의 Activity
            foreach (var activity in activities)
            {
                if (activity.Type != ActivityTypes.Message) continue;

                if (msg.message == null) msg.message = new Message();
                // 텍스트 메시지를 누적 시킴
                msg.message.text += "\n" + activity.Text;

                if (activity.Attachments != null && activity.Attachments.Count > 0)
                {
                    foreach (Attachment attachment in activity.Attachments)
                    {
                        switch (attachment.ContentType)
                        {
                            case "image/png":
                            case "image/jpeg":
                                if (msg.message.photo == null)
                                {
                                    msg.message.photo = new Photo
                                    {
                                        url = attachment.ContentUrl,
                                        width = 100,
                                        height = 100
                                    };
                                }
                                break;

                            case "application/vnd.microsoft.card.hero":
                                var heroCard = JsonConvert.DeserializeObject<HeroCard>(attachment.Content.ToString());
                                if (!string.IsNullOrEmpty(heroCard.Text.Trim()))
                                {
                                    msg.message.text += "\n\n" + heroCard.Text;
                                }
                                if (heroCard.Images != null)
                                {
                                    var img = heroCard.Images.FirstOrDefault();
                                    if (msg.message == null) msg.message = new Message();
                                    msg.message.photo = new Photo
                                    {
                                        url = img.Url,
                                        width = 1000,
                                        height = 1000
                                    };
                                }
                                if (heroCard.Buttons != null)
                                {
                                    var herobutton = heroCard.Buttons.Where(x => x.Type == ActionTypes.OpenUrl).FirstOrDefault();
                                    if (herobutton != null)
                                    {
                                        if (msg.message == null) msg.message = new Message();

                                        msg.message.message_button = new MessageButton
                                        {
                                            label = herobutton.Title,
                                            url = herobutton.Value.ToString()
                                        };
                                    }

                                    var heroactionbutton = heroCard.Buttons.Where(x => x.Type == ActionTypes.ImBack).ToList();
                                    if (msg.keyboard == null)
                                    {
                                        msg.keyboard = new Keyboard
                                        {
                                            buttons = new string[] { },
                                            type = "buttons"
                                        };
                                        List<string> buttons = new List<string>();
                                        foreach (var actionbutton in heroactionbutton)
                                        {
                                            buttons.Add(actionbutton.Value.ToString());
                                        }
                                        msg.keyboard.buttons = buttons.ToArray();
                                    }
                                }
                                break;
                        }
                    }
                }
            }

            return msg;
        }

        public static Models.MessageResponse DirectLineToKakao(Activity activity)
        {
            if (activity == null) return null;

            var msg = new Models.MessageResponse(); 

            if (activity.Type == ActivityTypes.Message)
            {
                if (msg.message == null) msg.message = new Message();
                // 텍스트 메시지 
                msg.message.text = activity.Text;

                // 이미지 
                if (activity.Attachments != null && activity.Attachments.Count > 0)
                {
                    foreach (Attachment attachment in activity.Attachments)
                    {
                        switch(attachment.ContentType)
                        {
                            case "image/png":
                            case "image/jpeg":
                                if (msg.message.photo == null)
                                {
                                    msg.message.photo = new Photo
                                    {
                                        url = attachment.ContentUrl
                                    };
                                }
                                break;

                            case "application/vnd.microsoft.card.hero":
                                var heroCard = JsonConvert.DeserializeObject<HeroCard>(attachment.Content.ToString());
                                var img = heroCard.Images.FirstOrDefault();
                                if (img != null && msg.message.photo == null)
                                {
                                    msg.message.photo = new Photo
                                    {
                                        url = img.Url
                                    };
                                }
                                if(heroCard.Buttons != null)
                                {
                                    List<string> buttons = new List<string>();
                                    foreach(CardAction action in heroCard.Buttons)
                                    {
                                        if (action.Type == ActionTypes.OpenUrl)
                                        {
                                            if (msg.message.message_button == null)
                                            {
                                                msg.message.message_button = new MessageButton
                                                {
                                                    label = action.Title,
                                                    url = action.Value.ToString()
                                                };
                                            }
                                        }
                                        else if (action.Type == ActionTypes.PostBack)
                                        {
                                            buttons.Add(action.Value.ToString());
                                        }
                                    }

                                    if (msg.keyboard == null)
                                    {
                                        msg.keyboard = new Keyboard
                                        {
                                            buttons = new string[] { },
                                            type = "buttons"
                                        };
                                        msg.keyboard.buttons = buttons.ToArray();
                                    }
                                }
                                break;
                        }
                    }
                }
            }

            return msg;
        }
    }
}