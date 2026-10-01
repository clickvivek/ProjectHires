using System;

namespace BusinessEntityAndDTO.DTO
{
    public class InitiateChatRequestDto
    {
        public long? UserId { get; set; }
        public long ChatUserId { get; set; }
    }

    public class InitiateChatResultDto
    {
        public bool CanChat { get; set; }
        public int DailyChatLimit { get; set; }
        public int UsedChatsToday { get; set; }
        public int RemainingChatsToday { get; set; }
        public bool IsExistingConversationToday { get; set; }
        public DateTime? NextSlotAvailableAtUtc { get; set; }
        public long? NextSlotWaitSeconds { get; set; }
        public string NextSlotWaitText { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
