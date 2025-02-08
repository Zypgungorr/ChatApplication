using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChatApplication.Models;
using System.Security.Claims;

namespace ChatApplication.Pages
{
    public class ChatPageModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ChatPageModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Group> Groups { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public List<Message> Messages { get; set; } = new();

        public int? GroupId { get; set; }
        public int? ReceiverId { get; set; }
        public int CurrentUserId { get; set; }
        public string ChatTitle { get; set; } = "";

        [BindProperty]
        public string MessageText { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(int? groupId, int? receiverId)
        {
            var userIdClaim = User.FindFirstValue("Id");
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToPage("/SignIn");
            }

            CurrentUserId = int.Parse(userIdClaim);

            Groups = await _context.Groups.Include(g => g.Members).ToListAsync();
            Users = await _context.Users.Where(u => u.Id != CurrentUserId).ToListAsync();

            if (groupId.HasValue)
            {
                GroupId = groupId;
                ChatTitle = _context.Groups.Find(groupId)?.GroupName ?? "Grup";
                Messages = await _context.Messages
                    .Where(m => m.GroupId == groupId)
                    .ToListAsync();
            }
            else if (receiverId.HasValue)
            {
                ReceiverId = receiverId;
                ChatTitle = _context.Users.Find(receiverId)?.Username ?? "Kişisel Sohbet";
                Messages = await _context.Messages
                    .Where(m => (m.SenderId == CurrentUserId && m.ReceiverId == receiverId) ||
                                (m.SenderId == receiverId && m.ReceiverId == CurrentUserId))
                    .ToListAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? groupId, int? receiverId)
        {
            var userIdClaim = User.FindFirstValue("Id");
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToPage("/SignIn");
            }

            int userId = int.Parse(userIdClaim);

            if (!string.IsNullOrWhiteSpace(MessageText))
            {
                var message = new Message
                {
                    SenderId = userId,
                    Text = MessageText,
                    CreatedAt = DateTime.UtcNow
                };

                if (groupId.HasValue)
                {
                    var group = await _context.Groups.FindAsync(groupId.Value);
                    if (group != null)
                    {
                        message.GroupId = groupId.Value;
                    }
                    else
                    {
                        return NotFound("Group not found.");
                    }
                }
                else if (receiverId.HasValue)
                {
                    var receiver = await _context.Users.FindAsync(receiverId.Value);
                    if (receiver != null)
                    {
                        message.ReceiverId = receiverId.Value;
                    }
                    else
                    {
                        return NotFound("Receiver not found.");
                    }
                }

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();
            }

            if (groupId.HasValue)
            {
                return RedirectToPage("/ChatPage", new { groupId = groupId });
            }
            else if (receiverId.HasValue)
            {
                return RedirectToPage("/ChatPage", new { receiverId = receiverId });
            }

            return Page();
        }

    }
}
