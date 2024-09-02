namespace Constants
{
    //public enum Equipe
    //{
    //    Jaune,
    //    Bleue,
    //}

    public enum CompetitionType
    {
        RoboCup,
        Eurobot2021,
        Eurobot2022,
        Cachan
    }

    public enum GameMode
    {
        Normal,
        Discovery,
        BallSearching,
    }

    public enum RobotDrivingMode
    {
        AutonomousPosition,
        AutonomousSpeed,
        XBoxSpeed,
        XBoxPosition,
    }

    public enum PlayingSide
    {
        Left,
        Right
    }

    public enum ObjectType
    {
        ObstacleLidar,
        Balise,
        Aruco,
        Poteau,
        Balle,
        RobotTeam1,
        RobotTeam2,
    }

    public enum Eurobot2022ArucoId
    {
        Rouge = 47,
        Vert = 36,
        Bleu = 13,
        Brun = 17,
    }

    public enum Eurobot2022TurbineId
    {
        Turbine_Avant = 17,
        Turbine_Arriere = 16,
    }

    public enum Eurobot2022SideColor { Purple, Yellow };
}
