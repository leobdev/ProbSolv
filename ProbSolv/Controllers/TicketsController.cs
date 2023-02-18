using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProbSolv.Data;
using ProbSolv.Extensions;
using ProbSolv.Models;
using ProbSolv.Models.Enums;
using ProbSolv.Services;
using ProbSolv.Services.Interfaces;

namespace ProbSolv.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<PSUser> _userManager;
        private readonly IPSLookupService _lookupService;
        private readonly IPSProjectService _projectService;
        private readonly IPSTicketService _ticketService;
        private readonly IPSFileService _fileService;

        public TicketsController(ApplicationDbContext context, UserManager<PSUser> userManager, IPSLookupService lookupService, IPSProjectService projectService, IPSTicketService ticketService, IPSFileService fileService)
        {
            _context = context;
            _userManager = userManager;
            _lookupService = lookupService;
            _projectService = projectService;
            _ticketService = ticketService;
            _fileService = fileService;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            var tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId);


            return View(tickets);
        }

        public async Task<IActionResult> AllTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId);

            if(User.IsInRole(nameof(Roles.Developer)) || User.IsInRole(nameof(Roles.Submitter)))
            {
                return View(tickets.Where(t => t.Archived is false));
            }
            else
            {
                return View(tickets);
            }             

        }


        public async Task<IActionResult> MyTickets()
        {
            PSUser psUser = await _userManager.GetUserAsync(User);      

            List<Ticket> tickets = await _ticketService.GetTicketsByUserIdAsync(psUser.Id, psUser.CompanyId);

            return View(tickets);

        }

        [Authorize(Roles="Admin,ProjectManager")]
        public async Task<IActionResult> UnassignedTickets()
        {
            
            int companyId = User.Identity.GetCompanyId().Value;
            string psuserId =  _userManager.GetUserId(User);

            List<Ticket> tickets = await _ticketService.GetUnassignedTicketsAsync(companyId);

            if (User.IsInRole(nameof(Roles.Admin)))
            {
                return View(tickets);
            }
            else
            {

                List<Ticket> pmTickets = new();

                foreach(Ticket ticket in tickets)
                {
                    if(await _projectService.IsAssignedProjectManagerAsync(psuserId, ticket.ProjectId))
                    {
                        pmTickets.Add(ticket);
                    }
                }

                return View(pmTickets);
            }
           
        }


        public async Task<IActionResult> ArchivedTickets()
        {
            //PSUser psUser = await _userManager.GetUserAsync(User);
            int companyId = User.Identity.GetCompanyId().Value;

            List<Ticket> tickets = await _ticketService.GetArchivedTicketsAsync(companyId);

            return View(tickets);
        }


        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
                        

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            PSUser psUser = await _userManager.GetUserAsync(User);
            int companyId = User.Identity.GetCompanyId().Value;

            if (User.IsInRole(nameof(Roles.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyAsync(companyId), "Id", "Name");


            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(psUser.Id), "Id", "Name");

            }
            
            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");            
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name");

            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId")] Ticket ticket)
        {
            PSUser psUser = await _userManager.GetUserAsync(User);
            int companyId = User.Identity.GetCompanyId().Value;

            if (ModelState.IsValid)
            {
                

                ticket.Created = DateTimeOffset.Now;
                ticket.OwnerUserId = psUser.Id;
                ticket.TicketStatusId = (await _ticketService.LookupTicketStatusIdAsync(nameof(PSTicketStatus.New))).Value;

                await _ticketService.AddNewTicketAsync(ticket);
                
                //TODO: Ticket History here

                //Ticket Notification
                return RedirectToAction(nameof(AllTickets));
            }

            if (User.IsInRole(nameof(Roles.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyAsync(companyId), "Id", "Name");

            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(psUser.Id), "Id", "Name");
            }

            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name");

            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
              
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _lookupService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);

            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,DeveloperUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                PSUser psUser = await _userManager.GetUserAsync(User);

                try
                {
                    ticket.Updated = DateTimeOffset.Now;

                    await _ticketService.UpdateTicketAsync(ticket);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                //TODO: Add ticket History 

                return RedirectToAction(nameof(AllTickets));
            }
            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _lookupService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);
            
            return View(ticket);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketComment([Bind("Id,TicketId,Comment")] TicketComment ticketComment)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ticketComment.UserId = _userManager.GetUserId(User);
                    ticketComment.Created = DateTimeOffset.Now; 

                    await _ticketService.AddTicketCommentAsync(ticketComment);

                }
                catch (Exception)
                {

                    throw;
                }
            }

            return RedirectToAction("Details", new { id = ticketComment.TicketId });

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;

            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                ticketAttachment.FileContentType = ticketAttachment.FormFile.ContentType;

                ticketAttachment.Created = DateTimeOffset.Now;
                ticketAttachment.UserId = _userManager.GetUserId(User);

                await _ticketService.AddTicketAttachmentAsync(ticketAttachment);
                statusMessage = "Success: New attachment added to Ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }



        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            string fileName = ticketAttachment.FileName;
            byte[] fileData = ticketAttachment.FileData;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }


        // GET: Tickets/Delete/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);
            ticket.Archived = true;            
            await _ticketService.UpdateTicketAsync(ticket);

            return RedirectToAction(nameof(AllTickets));
        }


        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

           

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ARestoreConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);
            ticket.Archived = false;
            await _ticketService.UpdateTicketAsync(ticket);

            return RedirectToAction(nameof(AllTickets));
        }




        private async Task<bool> TicketExists(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            return (await _ticketService.GetAllTicketsByCompanyAsync(companyId)).Any(t => t.Id == id);

        }
    }
}
