﻿namespace GaiaCore.Gaia
{
    public class GameStatus
    {
        Status status = Status.PREPARING;
    }

    public enum Status
    {
        PREPARING=0,
        RUNNING,
        ABORTED,
        ENDED
    }
}