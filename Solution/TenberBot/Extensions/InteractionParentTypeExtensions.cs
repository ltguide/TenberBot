using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenberBot.Data.Enums;

namespace TenberBot.Extensions;

public static class InteractionParentTypeExtensions
{
    public static InteractionParentLink GetLink(this InteractionParentType parentType)
    {
        return parentType switch
        {
            InteractionParentType.BotStatus => InteractionParentLink.Channel,
            InteractionParentType.Greeting => InteractionParentLink.Channel,
            InteractionParentType.Hug => InteractionParentLink.Channel,

            _ => throw new NotImplementedException(),
        };
    }
}
