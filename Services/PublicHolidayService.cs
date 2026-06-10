
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WUIAM.DTOs;
using WUIAM.Interfaces;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Services
{
    public class PublicHolidayService : IPublicHolidayService
    {
        private readonly IPublicHolidayRepository _publicHolidayRepository;

        public PublicHolidayService(IPublicHolidayRepository publicHolidayRepository)
        {
            _publicHolidayRepository = publicHolidayRepository;
        }

        public async Task<IEnumerable<PublicHoliday>> GetPublicHolidaysAsync()
        {
            return await _publicHolidayRepository.GetAllAsync();
        }

        public async Task<PublicHoliday> GetPublicHolidayByIdAsync(Guid id)
        {
            return await _publicHolidayRepository.GetByIdAsync(id);
        }

        public async Task<PublicHoliday> CreatePublicHolidayAsync(PublicHolidayDto publicHoliday)
        {
            return await _publicHolidayRepository.CreateAsync(publicHoliday);
        }

        public async Task<PublicHoliday> UpdatePublicHolidayAsync(Guid Id, PublicHolidayDto publicHoliday)
        {  var found = await GetPublicHolidayByIdAsync(Id);
            if (found == null) return null;

            found.Date = publicHoliday.Date;
            found.Name = publicHoliday.Name;
            found.IsRecurring = publicHoliday.IsRecurring;
            found.Description = publicHoliday.Description;

            return await _publicHolidayRepository.UpdateAsync(found);
        }

        public async Task<bool> DeletePublicHolidayAsync(Guid id)
        {
            return await _publicHolidayRepository.DeleteAsync(id);
        }
    }
}