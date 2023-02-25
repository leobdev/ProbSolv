// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using ProbSolv.Models;
using ProbSolv.Models.Enums;
using ProbSolv.Services.Interfaces;
using ProbSolv.Services;
using ProbSolv.Extensions;

namespace ProbSolv.Areas.Identity.Pages.Account
{

    public class RegisterByInviteModel : PageModel
    {
        private readonly SignInManager<PSUser> _signInManager;
        private readonly IPSProjectService _projectService;
        private readonly UserManager<PSUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IPSInviteService _inviteService;

        public RegisterByInviteModel(UserManager<PSUser> userManager,
            SignInManager<PSUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IPSInviteService inviteService,
            IPSProjectService projectService)
        {

            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _inviteService = inviteService;
            _projectService = projectService;

        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();


        public class InputModel
        {

            [Display(Name = "Avatar")]
            public byte[] ImageData { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [DisplayName("First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            
            [Display(Name = "Company Name")]
            public string Company { get; set; }

            [Required]
            [Display(Name = "CompanyId")]
            public int CompanyId { get; set; }

            [Required]
            [Display(Name = "ProjectId")]
            public int ProjectId { get; set; }


            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            //Use "id" to find the invite  
            Invite invite = await _inviteService.GetInviteAsync(id, companyId);

            //Load Inputmodel with Invite information according to inviteId
            Input.Email = invite.InviteeEmail;
            Input.FirstName = invite.InviteeFirstName;
            Input.LastName = invite.InviteeLastName;
            Input.Company = invite.Company.Name;
            Input.CompanyId = invite.CompanyId;
            Input.ProjectId = invite.ProjectId;

        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {

                var user = new PSUser
                {
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    UserName = Input.Email,
                    Email = Input.Email,
                    CompanyId = Input.CompanyId,
                    
                    
                };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {

                    await _projectService.AddUserToProjectAsync(user.Id, Input.ProjectId);

                    _logger.LogInformation("User created a new account with password.");

                    // -- Add new registrant a role of "NewUser" -- //
                    await _userManager.AddToRoleAsync(user, Roles.Submitter.ToString());

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Create", "Tickets");
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }

}
