namespace Tasks
{
    enum EventType : int
    {
        TaskName = 101, TaskDecription = 102, TaskDateStart = 103, TaskDateEnd = 104, TaskDirection = 105,
        TaskDo = 112, TaskUndo = 113, TaskDelete = 114, TaskUndelete = 115, TaskCopy = 116,
        StepAdd = 201, StepEdit = 202, StepDo = 203, StepUndo = 204, StepDelete = 205, StepUndelete = 206,
        CoopAdd = 301, CoppRemove = 302,
        MessageNew = 401,
        None = 0
    };
}