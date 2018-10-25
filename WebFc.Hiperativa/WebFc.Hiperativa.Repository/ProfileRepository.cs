﻿using Microsoft.AspNetCore.Hosting;
using WebFc.Hiperativa.Repository.Interface;
using UtilityFramework.Infra.Core.MongoDb.Business;
using WebFc.Hiperativa.Data.Entities;

namespace WebFc.Hiperativa.Repository
{
    public class ProfileRepository : BusinessBaseAsync<Profile>, IProfileRepository
    {
        public ProfileRepository(IHostingEnvironment env) : base(env)
        {
        }
    }
}