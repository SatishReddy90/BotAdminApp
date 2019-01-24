using Microsoft.Bot.Builder;
using SysforeAIBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SysforeAIBot.NlpDispatch
{
    public class SysforeAIBotAccessors
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// Contains the <see cref="ConversationState"/> and associated <see cref="IStatePropertyAccessor{T}"/>.
        /// </summary>
        /// <param name="conversationState">The state object that stores the counter.</param>
        public SysforeAIBotAccessors(ConversationState conversationState, UserState userState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
        }
        

        /// <summary>
        /// Gets the accessor name for the conversation flow property.
        /// </summary>
        /// <value>The accessor name for the conversation flow property.</value>
        /// <remarks>Accessors require a unique name.</remarks>
        public static string ConversationFlowName { get; } = "ConversationFlow";

        /// <summary>
        /// Gets the accessor name for the user profile property accessor.
        /// </summary>
        /// <value>The accessor name for the user profile property accessor.</value>
        /// <remarks>Accessors require a unique name.</remarks>
        public static string UserProfileName { get; } = "UserProfile";

        /// <summary>
        /// Gets or sets the <see cref="IStatePropertyAccessor{T}"/> for the conversation flow property.
        /// </summary>
        /// <value>
        /// The accessor stores the turn count for the conversation.
        /// </value>
        public IStatePropertyAccessor<ConversationFlow> ConversationFlowAccessor { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IStatePropertyAccessor{T}"/> for the user profile property.
        /// </summary>
        /// <value>
        /// The accessor stores the turn count for the conversation.
        /// </value>
        public IStatePropertyAccessor<UserProfile> UserProfileAccessor { get; set; }

        /// <summary>
        /// Gets the <see cref="ConversationState"/> object for the conversation.
        /// </summary>
        /// <value>The <see cref="ConversationState"/> object.</value>
        public ConversationState ConversationState { get; }

        /// <summary>
        /// Gets the <see cref="UserState"/> object for the conversation.
        /// </summary>
        /// <value>The <see cref="UserState"/> object.</value>
        public UserState UserState { get; }
    }
}
