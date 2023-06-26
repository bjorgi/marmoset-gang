using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace TaskManagerApp
{
    class Program
    {
static void Main(string[] args)
{
    string connectionString = "Server=DESKTOP-84N62PK\\SQLEXPRESS;Database=TaskManagerDB;User Id=treyh;Integrated Security=True;";

    List<TaskItem> tasks = new List<TaskItem>();

    // Fetch tasks from the SQL database initially
    FetchTasks(tasks, connectionString);

    while (true)
    {
        Console.WriteLine("Task Manager");
        Console.WriteLine("1. Add Task");
        Console.WriteLine("2. View Tasks");
        Console.WriteLine("3. Exit");
        Console.Write("Enter your choice (1-3): ");

        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                AddTask(tasks, connectionString);
                break;
            case "2":
                ViewTasks(tasks, connectionString);
                break;
            case "3":
                Console.WriteLine("Exiting Task Manager...");
                return;
            default:
                Console.WriteLine("Invalid choice. Please try again.");
                break;
        }

        Console.WriteLine();
    }
}

static void FetchTasks(List<TaskItem> tasks, string connectionString)
{
    // Fetch tasks from the SQL database
    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        connection.Open();

        // Create the SQL command
        string query = "SELECT * FROM Tasks";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int currentTaskId = reader.GetInt32(0);
                    string title = reader.GetString(1);
                    DateTime dueDate = reader.GetDateTime(2);
                    bool completed = reader.GetBoolean(3);

                    TaskItem task = new TaskItem(currentTaskId, title, dueDate, completed);
                    tasks.Add(task);
                }
            }
        }
    }
}

static void AddTask(List<TaskItem> tasks, string connectionString)
    {
        Console.WriteLine("Add Task");
        Console.Write("Enter task title: ");
        string title = Console.ReadLine();

        Console.Write("Enter due date (yyyy-mm-dd): ");
        if (DateTime.TryParse(Console.ReadLine(), out DateTime dueDate))
        {
            TaskItem task = new TaskItem(-1, title, dueDate, false);
            tasks.Add(task);

            // Insert the task into the SQL database
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Create the SQL command
                string query = "INSERT INTO Tasks (Title, DueDate, Completed) VALUES (@Title, @DueDate, @Completed)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Title", task.Title);
                    command.Parameters.AddWithValue("@DueDate", task.DueDate);
                    command.Parameters.AddWithValue("@Completed", task.Completed);

                    // Execute the command
                    command.ExecuteNonQuery();
                }
            }

            Console.WriteLine("Task added successfully.");
        }
        else
        {
            Console.WriteLine("Invalid due date format. Task not added.");
        }
    }

static void ViewTasks(List<TaskItem> tasks, string connectionString)
{
    Console.WriteLine("Tasks:");

    if (tasks.Count == 0)
    {
        Console.WriteLine("No tasks found.");
    }
    else
    {
        Console.WriteLine("ID\tTitle\t\tDue Date\tCompleted");
        Console.WriteLine("---------------------------------------------");

        // Fetch tasks from the SQL database
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            // Create the SQL command
            string query = "SELECT * FROM Tasks";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int currentTaskId = reader.GetInt32(0);
                        string title = reader.GetString(1);
                        DateTime dueDate = reader.GetDateTime(2);
                        bool completed = reader.GetBoolean(3);

                        TaskItem task = new TaskItem(currentTaskId, title, dueDate, completed);
                        tasks.Add(task);

                        string completionStatus = completed ? "Completed" : "Pending";
                        Console.WriteLine($"{currentTaskId}\t{title}\t{dueDate.ToShortDateString()}\t{completionStatus}");
                    }
                }
            }
        }
    }

    Console.WriteLine();
    Console.Write("Enter the ID of the task to perform an action (0 to cancel): ");
    if (int.TryParse(Console.ReadLine(), out int taskId))
    {
        if (taskId != 0)
        {
            // Find the task by ID
            TaskItem task = tasks.Find(t => t.TaskId == taskId);

            if (task != null)
            {
                Console.WriteLine();
                Console.WriteLine("1. Mark Task as Completed");
                Console.WriteLine("2. Delete Task");
                Console.WriteLine("3. Update Task Title");
                Console.WriteLine("4. Update Task Due Date");
                Console.WriteLine("5. Update Task Title and Due Date");
                Console.Write("Enter your choice (1-5): ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        task.Completed = true;

                        // Update the completion status in the SQL database
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();

                            // Create the SQL command
                            string query = "UPDATE Tasks SET Completed = @Completed WHERE TaskId = @TaskId";
                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@Completed", true);
                                command.Parameters.AddWithValue("@TaskId", task.TaskId);

                                // Execute the command
                                command.ExecuteNonQuery();
                            }
                        }

                        Console.WriteLine("Task marked as completed.");
                        break;
                    case "2":
                        tasks.Remove(task);

                        // Delete the task from the SQL database
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();

                            // Create the SQL command
                            string query = "DELETE FROM Tasks WHERE TaskId = @TaskId";
                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@TaskId", task.TaskId);

                                // Execute the command
                                command.ExecuteNonQuery();
                            }
                        }

                        Console.WriteLine("Task deleted successfully.");
                        break;
                    case "3":
                        Console.Write("Enter the new task title: ");
                        string newTitle = Console.ReadLine();
                        task.Title = newTitle;

                        // Update the task title in the SQL database
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();

                            // Create the SQL command
                            string query = "UPDATE Tasks SET Title = @Title WHERE TaskId = @TaskId";
                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@Title", newTitle);
                                command.Parameters.AddWithValue("@TaskId", task.TaskId);

                                // Execute the command
                                command.ExecuteNonQuery();
                            }
                        }

                        Console.WriteLine("Task title updated successfully.");
                        break;
                    case "4":
                        Console.Write("Enter the new due date (yyyy-mm-dd): ");
                        if (DateTime.TryParse(Console.ReadLine(), out DateTime newDueDate))
                        {
                            task.DueDate = newDueDate;

                            // Update the due date in the SQL database
                            using (SqlConnection connection = new SqlConnection(connectionString))
                            {
                                connection.Open();

                                // Create the SQL command
                                string query = "UPDATE Tasks SET DueDate = @DueDate WHERE TaskId = @TaskId";
                                using (SqlCommand command = new SqlCommand(query, connection))
                                {
                                    command.Parameters.AddWithValue("@DueDate", newDueDate);
                                    command.Parameters.AddWithValue("@TaskId", task.TaskId);

                                    // Execute the command
                                    command.ExecuteNonQuery();
                                }
                            }

                            Console.WriteLine("Task due date updated successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid due date format. Task due date not updated.");
                        }
                        break;
                    case "5":
                        Console.Write("Enter the new task title: ");
                        string newTitleAndDueDate = Console.ReadLine();
                        Console.Write("Enter the new due date (yyyy-mm-dd): ");
                        if (DateTime.TryParse(Console.ReadLine(), out DateTime newDueDateBoth))
                        {
                            task.Title = newTitleAndDueDate;
                            task.DueDate = newDueDateBoth;

                            // Update the task title and due date in the SQL database
                            using (SqlConnection connection = new SqlConnection(connectionString))
                            {
                                connection.Open();

                                // Create the SQL command
                                string query = "UPDATE Tasks SET Title = @Title, DueDate = @DueDate WHERE TaskId = @TaskId";
                                using (SqlCommand command = new SqlCommand(query, connection))
                                {
                                    command.Parameters.AddWithValue("@Title", newTitleAndDueDate);
                                    command.Parameters.AddWithValue("@DueDate", newDueDateBoth);
                                    command.Parameters.AddWithValue("@TaskId", task.TaskId);

                                    // Execute the command
                                    command.ExecuteNonQuery();
                                }
                            }

                            Console.WriteLine("Task title and due date updated successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid due date format. Task title and due date not updated.");
                        }
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Task action canceled.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Task not found.");
            }
        }
    }
}

        static void MarkTaskAsCompleted(List<TaskItem> tasks, string connectionString)
        {
            Console.WriteLine("Mark Task as Completed");
            Console.Write("Enter the ID of the task to mark as completed: ");
            if (int.TryParse(Console.ReadLine(), out int taskId))
            {
                // Find the task by ID
                TaskItem task = tasks.Find(t => t.TaskId == taskId);

                if (task != null)
                {
                    task.Completed = true;

                    // Update the completion status in the SQL database
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        // Create the SQL command
                        string query = "UPDATE Tasks SET Completed = @Completed WHERE TaskId = @TaskId";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Completed", true);
                            command.Parameters.AddWithValue("@TaskId", task.TaskId);

                            // Execute the command
                            command.ExecuteNonQuery();
                        }
                    }

                    Console.WriteLine("Task marked as completed.");
                }
                else
                {
                    Console.WriteLine("Task not found.");
                }
            }
            else
            {
                Console.WriteLine("Invalid task ID. Please try again.");
            }
        }

        static void DeleteTask(List<TaskItem> tasks, string connectionString)
        {
            Console.WriteLine("Delete Task");
            Console.Write("Enter the ID of the task to delete: ");
            if (int.TryParse(Console.ReadLine(), out int taskId))
            {
                // Find the task by ID
                TaskItem task = tasks.Find(t => t.TaskId == taskId);

                if (task != null)
                {
                    tasks.Remove(task);

                    // Delete the task from the SQL database
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        // Create the SQL command
                        string query = "DELETE FROM Tasks WHERE TaskId = @TaskId";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@TaskId", task.TaskId);

                            // Execute the command
                            command.ExecuteNonQuery();
                        }
                    }

                    Console.WriteLine("Task deleted successfully.");
                }
                else
                {
                    Console.WriteLine("Task not found.");
                }
            }
            else
            {
                Console.WriteLine("Invalid task ID. Please try again.");
            }
        }
    }

    class TaskItem
    {
        public int TaskId { get; set; }
        public string Title { get; set;}
        public DateTime DueDate { get; set; }
        public bool Completed { get; set; }

        public TaskItem(int taskId, string title, DateTime dueDate, bool completed)
        {
            TaskId = taskId;
            Title = title;
            DueDate = dueDate;
            Completed = completed;
        }
    }
}
