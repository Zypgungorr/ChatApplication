using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChatApplication.Models;
using ChatApplication.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace ChatApplication.Pages
{
    public class ChatPageModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _chatHub;

        public ChatPageModel(ApplicationDbContext context, IHubContext<ChatHub> chatHub)
        {
            _context = context;
            _chatHub = chatHub;
        }

        // Kullanıcıya ait bilgiler
        public List<Group> Groups { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public List<Message> Messages { get; set; } = new();

        public int? GroupId { get; set; }
        public int? ReceiverId { get; set; }
        public int CurrentUserId { get; set; }
        public string ChatTitle { get; set; } = "";

        [BindProperty]
        public string MessageText { get; set; } = "";
        [BindProperty]
        public IFormFile? FileUpload { get; set; }

        public class ChatListItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string? ProfileImageUrl { get; set; }
            public string LastMessage { get; set; } = "";
            public DateTime? LastMessageTime { get; set; }
            public bool IsGroup { get; set; }
        }

        public List<ChatListItem> ChatList { get; set; } = new();

        public class MessageViewModel
        {
            public string Text { get; set; } = "";
            public DateTime CreatedAt { get; set; }
            public int SenderId { get; set; }
            public string SenderName { get; set; } = "";
            public string SenderProfileImageUrl { get; set; } = "/wwwroot/img/people.png";
            public string? FileUrl { get; set; }
        }
        public List<MessageViewModel> MessagesVm { get; set; } = new();

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

            // Sohbet listesi için gruplar
            foreach (var group in Groups)
            {
                var lastMsg = await _context.Messages
                    .Where(m => m.GroupId == group.Id)
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefaultAsync();
                ChatList.Add(new ChatListItem
                {
                    Id = group.Id,
                    Name = group.GroupName ?? "Grup",
                    ProfileImageUrl = "/wwwroot/img/group_placeholder.png",
                    LastMessage = lastMsg?.Text ?? "",
                    LastMessageTime = lastMsg?.CreatedAt,
                    IsGroup = true
                });
            }
            // Sohbet listesi için kullanıcılar
            foreach (var user in Users)
            {
                var lastMsg = await _context.Messages
                    .Where(m => (m.SenderId == CurrentUserId && m.ReceiverId == user.Id) || (m.SenderId == user.Id && m.ReceiverId == CurrentUserId))
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefaultAsync();
                ChatList.Add(new ChatListItem
                {
                    Id = user.Id,
                    Name = user.Username ?? "Kullanıcı",
                    ProfileImageUrl = "/wwwroot/img/profile_placeholder.png",
                    LastMessage = lastMsg?.Text ?? "",
                    LastMessageTime = lastMsg?.CreatedAt,
                    IsGroup = false
                });
            }

            if (groupId.HasValue)
            {
                GroupId = groupId;
                ChatTitle = _context.Groups.Find(groupId)?.GroupName ?? "Grup";
                var messages = await _context.Messages
                    .Where(m => m.GroupId == groupId)
                    .OrderBy(m => m.CreatedAt)
                    .Include(m => m.Sender)
                    .ToListAsync();
                var fileDict = _context.Files.Where(f => f.GroupId == groupId).ToDictionary(f => f.CreatedAt, f => f.FilePath);
                MessagesVm = messages.Select(m => new MessageViewModel
                {
                    Text = m.Text ?? "",
                    CreatedAt = m.CreatedAt,
                    SenderId = m.SenderId,
                    SenderName = m.Sender?.Username ?? "Kullanıcı",
                    SenderProfileImageUrl = "/wwwroot/img/profile_placeholder.png",
                    FileUrl = fileDict.ContainsKey(m.CreatedAt) ? fileDict[m.CreatedAt] : null
                }).ToList();
            }
            else if (receiverId.HasValue)
            {
                ReceiverId = receiverId;
                ChatTitle = _context.Users.Find(receiverId)?.Username ?? "Kişisel Sohbet";
                var messages = await _context.Messages
                    .Where(m => (m.SenderId == CurrentUserId && m.ReceiverId == receiverId) ||
                                (m.SenderId == receiverId && m.ReceiverId == CurrentUserId))
                    .OrderBy(m => m.CreatedAt)
                    .Include(m => m.Sender)
                    .ToListAsync();
                var fileDict = _context.Files.Where(f => f.GroupId == null).ToDictionary(f => f.CreatedAt, f => f.FilePath);
                MessagesVm = messages.Select(m => new MessageViewModel
                {
                    Text = m.Text ?? "",
                    CreatedAt = m.CreatedAt,
                    SenderId = m.SenderId,
                    SenderName = m.Sender?.Username ?? "Kullanıcı",
                    SenderProfileImageUrl = "/wwwroot/img/profile_placeholder.png",
                    FileUrl = fileDict.ContainsKey(m.CreatedAt) ? fileDict[m.CreatedAt] : null
                }).ToList();
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

            int senderId = int.Parse(userIdClaim);
            string? filePath = null;

            // Dosya yükleme işlemi
            if (FileUpload != null && FileUpload.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(FileUpload.FileName);
                var fileSavePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(fileSavePath, FileMode.Create))
                {
                    await FileUpload.CopyToAsync(stream);
                }
                filePath = "/uploads/" + uniqueFileName;

                // File tablosuna ekle
                var fileEntity = new ChatApplication.Models.File
                {
                    SenderId = senderId,
                    FilePath = filePath,
                    GroupId = groupId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Files.Add(fileEntity);
                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrWhiteSpace(MessageText) || filePath != null)
            {
                var message = new Message
                {
                    SenderId = senderId,
                    Text = MessageText,
                    CreatedAt = DateTime.UtcNow
                };

                if (groupId.HasValue)
                {
                    var group = await _context.Groups.FindAsync(groupId.Value);
                    if (group != null)
                    {
                        message.GroupId = groupId.Value;
                        _context.Messages.Add(message);
                        await _context.SaveChangesAsync();

                        // Mesajı SignalR üzerinden gruba gönder
                        await _chatHub.Clients.Group($"Group_{groupId.Value}")
                            .SendAsync("ReceiveMessage", senderId, MessageText);
                    }
                    else
                    {
                        return NotFound("Grup bulunamadı.");
                    }
                }
                else if (receiverId.HasValue)
                {
                    var receiver = await _context.Users.FindAsync(receiverId.Value);
                    if (receiver != null)
                    {
                        message.ReceiverId = receiverId.Value;
                        _context.Messages.Add(message);
                        await _context.SaveChangesAsync();

                        // Mesajı SignalR üzerinden kullanıcıya gönder
                        await _chatHub.Clients.User(receiverId.Value.ToString())
                            .SendAsync("ReceiveMessage", senderId, MessageText);
                    }
                    else
                    {
                        return NotFound("Kullanıcı bulunamadı.");
                    }
                }
            }

            return new JsonResult(new { success = true });
        }



        // Kullanıcıların gruba katılım işlemi
        // public async Task<IActionResult> OnPostJoinGroupAsync(int groupId)
        // {
        //     var userIdClaim = User.FindFirstValue("Id");
        //     if (string.IsNullOrEmpty(userIdClaim))
        //     {
        //         return RedirectToPage("/SignIn");
        //     }

        //     // Kullanıcıyı gruba ekle
        //     await _chatHub.Clients.User(userIdClaim).SendAsync("JoinGroup", groupId);
        //     return RedirectToPage("/ChatPage", new { groupId });
        // }

        // // Kullanıcıların gruptan ayrılması işlemi
        // public async Task<IActionResult> OnPostLeaveGroupAsync(int groupId)
        // {
        //     var userIdClaim = User.FindFirstValue("Id");
        //     if (string.IsNullOrEmpty(userIdClaim))
        //     {
        //         return RedirectToPage("/SignIn");
        //     }

        //     // Kullanıcıyı gruptan çıkar
        //     await _chatHub.Clients.User(userIdClaim).SendAsync("LeaveGroup", groupId);
        //     return RedirectToPage("/ChatPage");
        // }
    }
}
