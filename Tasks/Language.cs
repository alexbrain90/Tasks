using System;

namespace Tasks
{
    static class Language
    {
        static public string Index = "Ru";

        #region Form - Main
        static public string Main_Caption = "Рабочие планы";

        static public string Main_UpdateCheck = "Проверка обновлений...";
        static public string Main_UpdateExist = "Доступна новая версия";
        static public string Main_UpdateNo = "Обновлений не найдено";

        static public string Main_NameInfo = "Наименование задачи";
        static public string Main_DescriptionInfo = "Описание задачи";
        static public string Main_DatesInfo = "Срок выполнения задачи"; static public string Main_DatesInfo1 = "С"; static public string Main_DatesInfo2 = "по";
        static public string Main_FilesInfo = "Прикрепленных файлов: ";
        static public string Main_CoopInfo = "Взаимодействие: ";
        static public string Main_CoopJustMe = "только я";
        static public string Main_CoopMeAnd = "я и ещё %N";
        static public string Main_ProgressInfo = "Процент выполнения задачи: ";
        static public string Main_MessagesInfo = "Комментарии";

        static public string Main_ButtonSelectWorker = "Выбрать работника";
        static public string Main_ButtonAddTask = "Добавить задачу";
        static public string Main_ButtonSaveTask = "Сохранить";
        static public string Main_ButtonCancelTask = "Отмена";
        static public string Main_ButtonDoTask = "Выполнить";
        static public string Main_ButtonShowFiles = "Посмотреть";

        static public string Main_SortListInfo = "Фильтр задач:";
        static public string[] Main_SortListNames = new string[] { "Активные", "Текущий месяц", "Предыдущий месяц", "Следующий месяц", "Все" };
        #endregion

        #region Form - Login
        static public string Login_Caption = "Вход в систему";
        static public string Login_UserName = "Имя пользователя";
        static public string Login_Password = "Пароль";

        static public string Login_OK = "Войти";
        static public string Login_Cancel = "Отмена";
        #endregion

        #region Ошибки
        static public string Error_Caption = "Ошибка";
        static public string Error_SaveSettings = "Не удалось выполнить сохранение параметов";
        static public string Error_UserNameNotFound = "Пользователь не найден";
        static public string Error_ServerConnection = "Не удалось подключиться к серверу";
        #endregion
    }
}