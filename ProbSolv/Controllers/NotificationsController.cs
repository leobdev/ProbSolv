using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.EntityFrameworkCore;
using ProbSolv.Data;
using ProbSolv.Extensions;
using ProbSolv.Models;
using ProbSolv.Models.Enums;
using ProbSolv.Models.ViewModels;
using ProbSolv.Services.Interfaces;

namespace ProbSolv.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<PSUser> _userManager;
        private readonly IPSNotificationService _notificationService;
        private readonly IPSProjectService _projectService;
        private readonly IPSRolesService _rolesService;
        private readonly IPSCompanyInfoService _companyInfoService;
        private readonly IPSTicketService _ticketService;

        public NotificationsController(ApplicationDbContext context, IEmailSender emailSender, UserManager<PSUser> userManager, IPSNotificationService notificationService, IPSProjectService projectService, IPSRolesService rolesService, IPSCompanyInfoService companyInfoService, IPSTicketService ticketService)
        {
            _context = context;
            _emailSender = emailSender;
            _userManager = userManager;
            _notificationService = notificationService;
            _projectService = projectService;
            _rolesService = rolesService;
            _companyInfoService = companyInfoService;
            _ticketService = ticketService;
        }

        // GET: Notifications
        public async Task<IActionResult> Inbox()
        {
            PSUser psUser = await _userManager.GetUserAsync(User);
            var mainRole = await _rolesService.GetUserMainRoleAsync(psUser);
            int companyId = User.Identity.GetCompanyId().Value;

            NotificationViewModel model = new();



            if (mainRole == Roles.Admin.ToString())
            {
                model.TicketsList = new SelectList(await _ticketService.GetAllTicketsByCompanyAsync(companyId), "Id", "Title");
            }
            else
            {
                model.TicketsList = new SelectList(await _ticketService.GetTicketsByUserIdAsync(psUser.Id, companyId), "Id", "Title");
            }

            //Fix Later
            model.RecipientLists = new SelectList(await _companyInfoService.GetAllMembersAsync(companyId), "Id","FullName");



            model.Notifications = (await _notificationService.GetAllNotificationsAsync()).Where(r => r.RecipientId == psUser.Id).ToList();

            return View(model);
        }

        public async Task<IActionResult> Outbox()
        {
            PSUser psUser = await _userManager.GetUserAsync(User);
            var mainRole = await _rolesService.GetUserMainRoleAsync(psUser);
            int companyId = User.Identity.GetCompanyId().Value;

            NotificationViewModel model = new();

            model.Notifications = (await _notificationService.GetUserNotificationsAsync(psUser.Id)).Where(r => r.SenderId == psUser.Id).ToList();

            if (mainRole == Roles.Admin.ToString())
            {
                model.TicketsList = new SelectList(await _ticketService.GetAllTicketsByCompanyAsync(companyId), "Id", "Title");
            }
            else
            {
                model.TicketsList = new SelectList(await _ticketService.GetTicketsByUserIdAsync(psUser.Id, companyId), "Id", "Title");
            }

            //Fix Later
            model.RecipientLists = new SelectList(await _companyInfoService.GetAllMembersAsync(companyId), "Id", "FullName");



            model.Notifications = await _notificationService.GetAllNotificationsAsync();

            return View(model);
        }

        // GET: Notifications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Notifications == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.Recipient)
                .Include(n => n.Sender)
                .Include(n => n.Ticket)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notification == null)
            {
                return NotFound();
            }

            await _notificationService.MarkAsNewAsync(notification);

            return View(notification);
        }

        // GET: Notifications/Create
        public IActionResult Create()
        {
            ViewData["RecipientId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description");
            return View();
        }

        // POST: Notifications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NotificationViewModel model)
        {
            if (ModelState.IsValid)
            {
                Notification notification = new Notification()
                {
                     Created = DateTimeOffset.Now,
                     Message = model.Message,
                     RecipientId = model.RecipientId,
                     SenderId = _userManager.GetUserId(User),
                     Title = model.Subject,
                     TicketId = model.TicketId,
                     Viewed = false

                };

                await _notificationService.AddNotificationAsync(notification);
                await _notificationService.SendEmailNotificationAsync(notification, model.Subject);

                return RedirectToAction(nameof(Inbox));
            }



            return RedirectToAction(nameof(Index));
        }

        // GET: Notifications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Notifications == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            ViewData["RecipientId"] = new SelectList(_context.Users, "Id", "Id", notification.RecipientId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", notification.SenderId);
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", notification.TicketId);
            return View(notification);
        }

        // POST: Notifications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TicketId,Title,Message,Created,RecipientId,SenderId,Viewed")] Notification notification)
        {
            if (id != notification.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notification);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotificationExists(notification.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RecipientId"] = new SelectList(_context.Users, "Id", "Id", notification.RecipientId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", notification.SenderId);
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", notification.TicketId);
            return View(notification);
        }

        // GET: Notifications/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Notifications == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.Recipient)
                .Include(n => n.Sender)
                .Include(n => n.Ticket)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // POST: Notifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Notifications == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Notifications'  is null.");
            }
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NotificationExists(int id)
        {
            return _context.Notifications.Any(e => e.Id == id);
        }
    }
}
