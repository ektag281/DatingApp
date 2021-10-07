using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DatingApp.Api.DTOs;
using DatingApp.Api.Entities;
using DatingApp.Api.Helpers;
using DatingApp.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Api.Data
{
    public class MessageRepositoy : IMessageRepository
    {
        private readonly DataContext _context ;
        private readonly IMapper _mapper;
        public MessageRepositoy(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
                        .Include(c => c.Sender)
                        .Include(c => c.Recipient)
                        .SingleOrDefaultAsync(c => c.Id == id);
        }

        public async Task<PagedList<MessageDto>> GetMessageForUser(MessageParams messageParams)
        {
            var query = _context.Messages.OrderByDescending(x => x.MessageSent).AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where
                (c => c.Recipient.UserName == messageParams.Username &&
                      c.RecipientDeleted == false),
                "Outbox" => query.Where
                (c => c.Sender.UserName == messageParams.Username &&
                    c.SenderDeleted == false),
                _ => query.Where(c => c.Recipient.UserName ==
                    messageParams.Username && c.RecipientDeleted== false &&
                    c.DateRead == null)
            };
            
            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);
            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber,messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName,
         string recipientUsername)
        {
            var messages =  await _context.Messages
                            .Include(u => u.Sender).ThenInclude(p => p.Photos)
                            .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                            .Where(c => c.Recipient.UserName == currentUserName  
                                    && c.RecipientDeleted == false
                                    && c.SenderUsername == recipientUsername
                                    || c.Recipient.UserName == recipientUsername
                                    && c.SenderUsername == currentUserName
                                    && c.SenderDeleted == false)
                            .OrderBy(c => c.MessageSent)
                            .ToListAsync();
            
            var unreadMessages = messages.Where(c => c.DateRead == null &&
                                          c.Recipient.UserName == currentUserName)
                                          .ToList();

            if(unreadMessages.Any())
            {
                foreach (var item in unreadMessages)
                {
                    item.DateRead = System.DateTime.Now;
                }

                await _context.SaveChangesAsync();
            }

            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}