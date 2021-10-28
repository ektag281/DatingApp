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
            var query = _context.Messages.
            OrderByDescending(x => x.MessageSent).
            ProjectTo<MessageDto>(_mapper.ConfigurationProvider).
            AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where
                (c => c.RecipientUserName == messageParams.Username &&
                      c.RecipientDeleted == false),
                "Outbox" => query.Where
                (c => c.SenderUsername == messageParams.Username &&
                    c.SenderDeleted == false),
                _ => query.Where(c => c.RecipientUserName ==
                    messageParams.Username && c.RecipientDeleted== false &&
                    c.DateRead == null)
            };
            
            return await PagedList<MessageDto>.CreateAsync(query, messageParams.PageNumber,messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName,
         string recipientUsername)
        {
            var messages =  await _context.Messages
                            .Where(c => c.Recipient.UserName == currentUserName  
                                    && c.RecipientDeleted == false
                                    && c.SenderUsername == recipientUsername
                                    || c.Recipient.UserName == recipientUsername
                                    && c.SenderUsername == currentUserName
                                    && c.SenderDeleted == false)
                            .OrderBy(c => c.MessageSent)
                            .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                            .ToListAsync();
            
            var unreadMessages = messages.Where(c => c.DateRead == null &&
                                          c.RecipientUserName == currentUserName)
                                          .ToList();

            if(unreadMessages.Any())
            {
                foreach (var item in unreadMessages)
                {
                    item.DateRead = System.DateTime.UtcNow;
                }
            }

            return (messages);
        }

        void IMessageRepository.AddGroup(Group group)
        {
           _context.Groups.Add(group);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups.Include(c => c.Connections)
            .FirstOrDefaultAsync(c => c.Name == groupName);
        }

        void IMessageRepository.RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

        async Task<Group> IMessageRepository.GetGroupForConnection(string connectionId)
        {
             return await _context.Groups.Include(c => c.Connections)
             .Where(c => c.Connections.Any(c => c.ConnectionId == connectionId))
             .FirstOrDefaultAsync();
        }
    }
}