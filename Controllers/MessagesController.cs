using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.Api.DTOs;
using DatingApp.Api.Entities;
using DatingApp.Api.Extensions;
using DatingApp.Api.Helpers;
using DatingApp.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Api.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public MessagesController(IUnitOfWork unitOfWork,
                                IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var userName = User.GetUserName();

            if (userName == createMessageDto.RecipientUsername.ToLower())
                return BadRequest("You can not send message to yourself");

            var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(userName);
            var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUserName = recipient.UserName,
                Content = createMessageDto.Content
            };

            _unitOfWork.MessageRepository.AddMessage(message);

            if(await _unitOfWork.Complete()) return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageForUser([FromQuery]MessageParams messageParams)
        {
            messageParams.Username = User.GetUserName();

            var messages = await _unitOfWork.MessageRepository.GetMessageForUser(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage,messageParams.PageSize,
            messages.TotalCounts,messages.TotalPages);

            return messages;

        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUserName = User.GetUserName();

            return Ok(await _unitOfWork.MessageRepository.GetMessageThread(currentUserName,username));


        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUserName();

            //Fetching my id
            var message = await _unitOfWork.MessageRepository.GetMessage(id);

            //Checking username in sender also at receiver username
            if(message.Sender.UserName != username && message.Recipient.UserName != username)
            return Unauthorized();

            //Delete message from sender user 
            if(message.Sender.UserName == username) 
            message.SenderDeleted = true;

            //Delete message from Recipient user 
            if(message.Recipient.UserName == username) 
            message.RecipientDeleted = true;

            //Check if message deleted then delete from repository
            if(message.SenderDeleted && message.RecipientDeleted)
            _unitOfWork.MessageRepository.DeleteMessage(message);

            if(await _unitOfWork.Complete()) 
            return Ok();

            return BadRequest("Problem While Deliting");
        }
    }
}