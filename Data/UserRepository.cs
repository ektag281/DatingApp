using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DatingApp.Api.DTOs;
using DatingApp.Api.Entities;
using DatingApp.Api.Helpers;
using DatingApp.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Api.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            return await _context.Users
            .Where(x => x.UserName == username)
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            // var query =  _context.Users
            // .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            // .AsNoTracking()
            // .AsQueryable();
            //   return await PagedList<MemberDto>.CreateAsync(
            //     query,
            //     userParams.PageNumber,userParams.PageSize);

            var query = _context.Users.AsQueryable();

            query = query.Where(u => u.UserName != userParams.CurrentUserName);
            query = query.Where(u => u.Gender == userParams.Gender);

            var minDOB = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDOB = DateTime.Today.AddYears(-userParams.MinAge);

            query = query.Where(c => c.DateOfBirth >= minDOB && c.DateOfBirth <= maxDOB);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(c => c.Created),//Create is case -1 
                _ => query.OrderByDescending(c => c.LastActive), // this is default statement in c# 8
            };
            return await PagedList<MemberDto>.CreateAsync(
                query.ProjectTo<MemberDto>(_mapper.ConfigurationProvider).
                AsNoTracking(),
                userParams.PageNumber,userParams.PageSize);

           
        }

        public async Task<IEnumerable<AppUser>> GetUserAsync()
        {
            return await _context.Users
                                 .Include(c => c.Photos).ToListAsync();
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                                .Include(c => c.Photos).SingleOrDefaultAsync(c => c.UserName == username);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Update(AppUser appUser)
        {
            _context.Entry(appUser).State = EntityState.Modified;
        }
    }
}