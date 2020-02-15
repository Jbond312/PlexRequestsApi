﻿using System.Linq;
using AutoMapper;
using PlexRequests.ApiRequests.Users.Models.Detail;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserRow, UserDetailModel>()
                .ForMember(x => x.Id, x => x.MapFrom(y => y.UserId))
                .ForMember(x => x.LastLogin, x => x.MapFrom(y => y.LastLoginUtc))
                .ForMember(x => x.Roles, x => x.MapFrom(y => y.UserRoles.Select(ur => ur.Role)));
        }
    }
}
