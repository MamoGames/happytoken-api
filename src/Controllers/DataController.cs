using HappyTokenApi.Data.Config;
using HappyTokenApi.Data.Core;
using HappyTokenApi.Data.Core.Entities;
using HappyTokenApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HappyTokenApi.Controllers
{
    [Route("[controller]")]
    public class DataController : Controller
    {
        protected readonly CoreDbContext m_CoreDbContext;

        protected readonly ConfigDbContext m_ConfigDbContext;

        protected List<RecordData> updatedDataRecords;

        public DataController(CoreDbContext coreDbContext, ConfigDbContext configDbContext)
        {
            m_CoreDbContext = coreDbContext;
            m_ConfigDbContext = configDbContext;

            this.updatedDataRecords = new List<RecordData>();
        }

        protected void AddDataToReturnList(string key, object obj) 
        {
            // TODO: remove existing record data with the same key

            this.updatedDataRecords.Add(new RecordData
            {
                Key = key,
                Hash = "123",
                Data = obj,
            });
        }

        protected IActionResult RequestResult(int statusCode, object content) 
        {
            return Ok(new RequestResult
            {
                Content = content,
                StatusCode = statusCode,
                Data = this.updatedDataRecords.ToArray(),
            });
        }

        [Authorize]
        [HttpPost("status1")]
        public async Task<IActionResult> DataStatus()
        {
            return Ok("test");
        }

        [Authorize]
        [HttpPost("cake1")]
        public async Task<Cake> DataCake()
        {
            return new Cake
            {
                Gold = 10,
            };
        }

    }
}
