using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _402410745_Staybnb_assignment.Data;
using _402410745_Staybnb_assignment.Models;
using _402410745_Staybnb_assignment.Services;
using _402410745_Staybnb_assignment.ViewModels;

namespace _402410745_Staybnb_assignment.Controllers;

// Simple database-backed inbox: a list of people on the left, the conversation on the right.
[Authorize]
public class MessagesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly AuditService _audit;

    public MessagesController(ApplicationDbContext db, UserManager<ApplicationUser> users, AuditService audit)
    {
        _db = db;
        _users = users;
        _audit = audit;
    }

    public async Task<IActionResult> Index(string with)
    {
        var me = _users.GetUserId(User);
        var model = new InboxViewModel();

        var mine = await _db.Messages
            .Where(m => m.SenderId == me || m.ReceiverId == me)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();

        // Everyone I have talked to (most recent first)
        var personIds = mine.Select(m => m.SenderId == me ? m.ReceiverId : m.SenderId).Distinct().ToList();
        if (!string.IsNullOrEmpty(with) && with != me && !personIds.Contains(with))
            personIds.Insert(0, with);   // starting a new conversation

        var people = await _users.Users.Where(u => personIds.Contains(u.Id)).ToListAsync();
        foreach (var id in personIds)
        {
            var person = people.FirstOrDefault(u => u.Id == id);
            if (person != null) model.People.Add(person);
        }

        foreach (var m in mine.Where(m => m.ReceiverId == me && !m.IsRead))
        {
            if (model.UnreadFrom.ContainsKey(m.SenderId)) model.UnreadFrom[m.SenderId]++;
            else model.UnreadFrom[m.SenderId] = 1;
        }

        if (!string.IsNullOrEmpty(with))
        {
            model.Other = model.People.FirstOrDefault(u => u.Id == with);
            if (model.Other != null)
            {
                model.Conversation = mine.Where(m => m.SenderId == with || m.ReceiverId == with).OrderBy(m => m.Timestamp).ToList();

                // Opening a conversation marks its received messages as read.
                var unread = model.Conversation.Where(m => m.ReceiverId == me && !m.IsRead).ToList();
                foreach (var m in unread) m.IsRead = true;
                if (unread.Count > 0) await _db.SaveChangesAsync();
                model.UnreadFrom.Remove(with);
            }
        }
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(string receiverId, string content)
    {
        var me = _users.GetUserId(User);
        var receiver = await _users.FindByIdAsync(receiverId);

        if (receiver == null || receiver.Id == me || string.IsNullOrWhiteSpace(content))
            return RedirectToAction("Index");

        if (content.Length > 2000) content = content.Substring(0, 2000);

        _db.Messages.Add(new Message { SenderId = me, ReceiverId = receiver.Id, Content = content });
        await _db.SaveChangesAsync();

        var sender = await _users.FindByIdAsync(me);
        await _audit.NotifyAsync(receiver.Id, "New Message", "New message from " + sender.FullName, NotificationType.Info, "/Messages?with=" + me);

        return RedirectToAction("Index", new { with = receiverId });
    }
}
