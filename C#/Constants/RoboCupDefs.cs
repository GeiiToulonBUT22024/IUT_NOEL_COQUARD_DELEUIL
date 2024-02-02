namespace Constants
{
    public enum RefBoxCommand
    {
        START,
        STOP,
        DROP_BALL,
        HALF_TIME,
        END_GAME,
        GAME_OVER,
        PARK,
        FIRST_HALF,
        SECOND_HALF,
        FIRST_HALF_OVER_TIME,
        RESET,
        WELCOME,
        KICKOFF,
        FREEKICK,
        GOALKICK,
        THROWIN,
        CORNER,
        PENALTY,
        GOAL,
        SUBGOAL,
        REPAIR,
        YELLOW_CARD,
        DOUBLE_YELLOW,
        RED_CARD,
        SUBSTITUTION,
        IS_ALIVE,


        //Added commands for debug
        GOTO,
        PLAYLEFT,
        PLAYRIGHT,
    }

    public enum RoboCupPoste
    {
        Unassigned = 0,
        GoalKeeper = 1,
        DefenderLeft = 2,
        DefenderCenter = 3,
        DefenderRight = 4,
        MidfielderLeft = 5,
        MidfielderCenter = 6,
        MidfielderRight = 7,
        ForwardLeft = 8,
        ForwardCenter = 9,
        ForwardRight = 10,
    }


    public enum BallHandlingState
    {
        NoBall,
        HasBall,
        PassInProgress,
        ShootInProgress,
    }

    public static class RoboCupField
    {
        public static double FieldLength = 22;
        public static double FieldWidth = 14;
        public static double StadiumLength = 26;
        public static double StadiumWidth = 18;
    }

    public enum GameState
    {
        STOPPED,
        STOPPED_GAME_POSITIONING,
        PLAYING,
    }

    public enum StoppedGameAction
    {
        NONE,
        KICKOFF,
        KICKOFF_OPPONENT,
        FREEKICK,
        FREEKICK_OPPONENT,
        GOALKICK,
        GOALKICK_OPPONENT,
        THROWIN,
        THROWIN_OPPONENT,
        CORNER,
        CORNER_OPPONENT,
        PENALTY,
        PENALTY_OPPONENT,
        PARK,
        DROPBALL,
        GOTO,
        GOTO_OPPONENT,
    }
}
