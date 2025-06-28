using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ChatApplication.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace ChatApplication.Pages
{
    public class GroupPageModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public GroupPageModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Group> Groups { get; set; } = new List<Group>();

    [BindProperty]
    public string? GroupName { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = User.FindFirstValue("Id");

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/SignIn");
        }

        var user = await _context.Users.FindAsync(int.Parse(userId));

        if (user == null)
        {
            return RedirectToPage("/Error");
        }

        Groups = _context.Groups
            .Include(g => g.Members)
            .Where(g => g.Members == null || g.Members.Any(m => m.UserId == user.Id))
            .ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostCreateGroupAsync()
    {
        if (string.IsNullOrEmpty(GroupName))
        {
            return Page(); // If group name is empty, return the same page
        }

        var newGroup = new Group
        {
            GroupName = GroupName,
            CreatedAt = DateTime.UtcNow
        };

        _context.Groups.Add(newGroup);
        await _context.SaveChangesAsync();

        // Add the current user as a member of the newly created group
        var userId = User.FindFirstValue("Id");
        var user = await _context.Users.FindAsync(int.Parse(userId!));

        var groupMember = new GroupMember
        {
            GroupId = newGroup.Id,
            UserId = user!.Id,
            JoinedAt = DateTime.UtcNow
        };

        _context.GroupMembers.Add(groupMember);
        await _context.SaveChangesAsync();

        return RedirectToPage("/GroupPage");
    }
}

}
