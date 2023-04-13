using Microsoft.AspNetCore.Html;
using ProbSolv.Models.Enums;
using ProbSolv.Services.Interfaces;

namespace ProbSolv.Services
{
    public class PSBadgeService : IPSBadgeService
    {

        public string GetFonticonByHistory(string historyProperty)
        {
            //new ticket
            if (historyProperty == "")
            {

                return "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" class=\"feather feather-plus\"><line x1=\"12\" y1=\"5\" x2=\"12\" y2=\"19\"></line><line x1=\"5\" y1=\"12\" x2=\"19\" y2=\"12\"></line></svg>";
            }

            //Title
            if (historyProperty == "Title")
            {
                return "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" class=\"feather feather-type\"><polyline points=\"4 7 4 4 20 4 20 7\"/><line x1=\"9\" y1=\"20\" x2=\"15\" y2=\"20\"/><line x1=\"12\" y1=\"4\" x2=\"12\" y2=\"20\"/></svg>";
            }

            //Description
            if (historyProperty == "Description")
            {
                return "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" class=\"feather feather-type\"><polyline points=\"4 7 4 4 20 4 20 7\"/><line x1=\"9\" y1=\"20\" x2=\"15\" y2=\"20\"/><line x1=\"12\" y1=\"4\" x2=\"12\" y2=\"20\"/></svg>";
            }

            //Priority
            if (historyProperty == "TicketPriority")
            {
                return "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" class=\"feather feather-type\"><polyline points=\"4 7 4 4 20 4 20 7\"/><line x1=\"9\" y1=\"20\" x2=\"15\" y2=\"20\"/><line x1=\"12\" y1=\"4\" x2=\"12\" y2=\"20\"/></svg>";
            }

            //Status
            if (historyProperty == "TicketStatus")
            {
                return "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" class=\"feather feather-bar-chart\"><line x1=\"12\" y1=\"20\" x2=\"12\" y2=\"10\"/><line x1=\"18\" y1=\"20\" x2=\"18\" y2=\"4\"/><line x1=\"6\" y1=\"20\" x2=\"6\" y2=\"16\"/></svg>";
            }

            //Type
            if (historyProperty == "TicketTypeId")
            {
                return "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" class=\"feather feather-type\"><polyline points=\"4 7 4 4 20 4 20 7\"/><line x1=\"9\" y1=\"20\" x2=\"15\" y2=\"20\"/><line x1=\"12\" y1=\"4\" x2=\"12\" y2=\"20\"/></svg>";
            }

            //Dev
            if (historyProperty == "Developer")
            {
                return "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" class=\"feather feather-user-check\"><path d=\"M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2\"/><circle cx=\"8.5\" cy=\"7\" r=\"4\"/><polyline points=\"17 11 19 13 23 9\"/></svg>";
            }

            //Description
            if (historyProperty == "TicketComment")
            {
                return "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" class=\"feather feather-type\"><polyline points=\"4 7 4 4 20 4 20 7\"/><line x1=\"9\" y1=\"20\" x2=\"15\" y2=\"20\"/><line x1=\"12\" y1=\"4\" x2=\"12\" y2=\"20\"/></svg>";
            }

            // Description
            if (historyProperty == "TicketAttachment")
            {
                return "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" class=\"feather feather-image\"><rect x=\"3\" y=\"3\" width=\"18\" height=\"18\" rx=\"2\" ry=\"2\"/><circle cx=\"8.5\" cy=\"8.5\" r=\"1.5\"/><polyline points=\"21 15 16 10 5 21\"/></svg>";
            }

            return null;

        }

        public string GetFunticonColorByHistory(string historyProperty)
        {
            //new ticket
            if (historyProperty == "")
            {

                return "t-success";
            }

            //Title
            if (historyProperty == "Title")
            {
                return "t-warning";
            }

            //Description
            if (historyProperty == "Description")
            {
                return "t-primary";
            }

            //Priority
            if (historyProperty == "TicketPriority")
            {
                return "t-danger";
            }

            //Status
            if (historyProperty == "TicketStatus")
            {
                return "t-secondary";
            }

            //Type
            if (historyProperty == "TicketTypeId")
            {
                return "t-secondary";
            }

            //Dev
            if (historyProperty == "Developer")
            {
                return "t-dark";
            }

            //Description
            if (historyProperty == "TicketAttachment")
            {
                return "t-primary";
            }

            //Type
            if (historyProperty == "TicketComment")
            {
                return "t-secondary";
            }

            return null;
        }



        public string GetPriorityBadge(string priorityName)
        {
            //Low
            //Medium
            //High
            //Urgent

            if (priorityName == PSProjectPriority.Low.ToString() || priorityName == PSTicketPriority.Low.ToString())
            {
                return "bg-success";
            }

            if (priorityName == PSProjectPriority.Medium.ToString() || priorityName == PSTicketPriority.Medium.ToString())
            {
                return "bg-warning";
            }

            if (priorityName == PSProjectPriority.High.ToString() || priorityName == PSTicketPriority.High.ToString())
            {
                return "bg-danger";
            }


            return "bg-dark ";


        }


        public string GetPriorityColor(string priorityName)
        {
            //Low
            //Medium
            //High
            //Urgent

            if (priorityName == PSProjectPriority.Low.ToString() || priorityName == PSTicketPriority.Low.ToString())
            {
                return "btn-success";
            }

            if (priorityName == PSProjectPriority.Medium.ToString() || priorityName == PSTicketPriority.Medium.ToString())
            {
                return "btn-secondary";
            }

            if (priorityName == PSProjectPriority.High.ToString() || priorityName == PSTicketPriority.High.ToString())
            {
                return "btn-warning";
            }


            return "btn-danger";


        }

        public string GetStatusBadge(string statusName)
        {
                //New,
                //Development,
                //Testing,
                //Resolved

            if (statusName == PSTicketStatus.New.ToString())
            {
                return "btn-outline-success";
            }

            if (statusName == PSTicketStatus.Development.ToString())
            {
                return "btn-outline-primary";
            }

            if (statusName == PSTicketStatus.Testing.ToString())
            {
                return "btn-outline-warning";
            }


            return "btn-outline-dark";


        }


    }
}

