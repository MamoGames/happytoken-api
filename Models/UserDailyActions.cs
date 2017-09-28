using System;
using System.Collections.Generic;

namespace HappyTokenApi.Models
{
    public class UserDailyActions
    {
        public DateTime CurrentDate { get; set; }

        public string[] VisitedUserIds { get; set; }

        public string[] GiftedCakeUserIds { get; set; }

        protected void ResetRecords()
        {
            this.VisitedUserIds = new string[0];
            this.GiftedCakeUserIds = new string[0];
        }

        /// <summary>
        /// Routine check if the data need update
        /// </summary>
        /// <returns>true if there were update of data</returns>
        public bool Update()
        {
            if (this.CurrentDate.Date != DateTime.UtcNow.Date)
            {
                this.ResetRecords();

                System.Diagnostics.Debug.WriteLine("Updated");

                this.CurrentDate = DateTime.UtcNow;

                return true;
            }

            return false;
        }
            
    }
}
