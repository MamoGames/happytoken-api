using System;
namespace HappyTokenApi.Models
{
    /// <summary>
    /// Stat name end with underscore "_" has additional value at the end 
    /// Stat name start with "F_" means value is float and stored value has been multiplied by 1000
    /// This enum should not be used as integer, always use the string value since some of them need extra string to be filled at the end.
    /// </summary>
    public enum UserStatType
    {
        USER_LEVEL = 1,
        MINI_GAME_LEVEL_,
        GOLD_GAINED,
        GEM_GAINED,
        GOLD_SPENT,
        GEM_SPENT,
        HAPPY_TOKENS_GAINED,
        HAPPY_TOKENS_SPENT,
        F_IAP_AMOUNT,
        IAP_CREDIT_AMOUNT,
        QUEST_COMPLETED_,
        QUEST_COMPLETED_TOTAL,
        MINI_GAME_WIN_TOTAL,
        MINI_GAME_WIN_,
        MINI_GAME_COMPLETED_TOTAL,
        MINI_GAME_COMPLETED_,
        CAKE_BAKED_TOTAL,
        CAKE_BAKED_,
        CAKE_GIFTED_TOTAL,
        CAKE_GIFTED_,
        FRIEND_VISITED,
        BUILDING_UPGRADE_TOTAL,
        BUILDING_UPGRADE_,
    }
}