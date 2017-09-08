using System;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace HappyTokenApi.Controllers
{
    public static class Extensions
    {
        /// <summary>
        /// Used to ensure the user requested in the method matches the user in the JWT Claim.
        /// For example; A user can only request their own profile
        /// </summary>
        public static bool IsClaimantUserId(this Controller controller, string userId)
        {
            // Ensure we have both the user and Claims data
            if (string.IsNullOrEmpty(userId) || !controller.User.Claims.Any())
            {
                return false;
            }

            // Grab the UserId from the Claim
            var claimsUserId = controller.User.Claims.First().Value;

            // Ensure the UserId and Claim UserId match
            return !string.IsNullOrEmpty(claimsUserId) && userId == claimsUserId;
        }

        /// <summary>
        /// Get the UserId from the User Claims in the controller
        /// </summary>
        public static string GetClaimantUserId(this Controller controller)
        {
            // Ensure we have the Claims data & grab the UserId from the Claim
            return controller.User.Claims.Any() ? controller.User.Claims.First().Value : string.Empty;
        }

        /// <summary>
        /// Validates the UserId UUID/GUID
        /// </summary>
        /// <remarks>
        /// Example UserId: 96658fe3-f16d-4340-b36d-fd003caee7be
        /// </remarks>
        public static bool IsValidUserId(this Controller controller, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            return Guid.TryParse(userId, out _);
        }
    }
}
