using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ToDoList
{
	class Program
	{
		private const string ConnectionString =
			"Server=LAPTOP-CA65ONBH\\SQLEXPRESS;Database=TodoDb;Trusted_Connection=True;TrustServerCertificate=True;";

		static void Main()
		{
			while (true)
			{
				Console.WriteLine("\n===== TO-DO LIST  =====");
				Console.WriteLine("1. Add tasks ");
				Console.WriteLine("2. Show tasks");
				Console.WriteLine("3. Mark task as done");
				Console.WriteLine("4. Delete task");
				Console.WriteLine("5. Exit");
				Console.Write("Enter choice: ");

				string choice = Console.ReadLine();

				switch (choice)
				{
					case "1":
						AddTasks();
						break;
					case "2":
						ShowTasks();
						break;
					case "3":
						MarkTaskAsDone();
						break;
					case "4":
						DeleteTask();
						break;
					case "5":
						Console.WriteLine("Goodbye!");
						return;
					default:
						Console.WriteLine("Invalid choice!");
						break;
				}
			}
		}

		// ============ ADD MULTIPLE TASKS ============
		static void AddTasks()
		{
			var tasks = new List<(string Title, string Description)>();

			Console.WriteLine("\nEnter tasks. Leave title empty to stop.");

			while (true)
			{
				Console.Write("\nTask title (blank = finish): ");
				string title = Console.ReadLine();

				if (string.IsNullOrWhiteSpace(title))
					break;

				Console.Write("Description (optional): ");
				string description = Console.ReadLine();

				tasks.Add((title.Trim(), description?.Trim()));
			}

			if (tasks.Count == 0)
			{
				Console.WriteLine("No tasks entered.");
				return;
			}

			using (SqlConnection conn = new SqlConnection(ConnectionString))
			{
				conn.Open();

				string sql = "INSERT INTO Tasks (TaskTitle, Description, IsDone) VALUES (@Title, @Description, 0);";

				foreach (var task in tasks)
				{
					using (SqlCommand cmd = new SqlCommand(sql, conn))
					{
						cmd.Parameters.AddWithValue("@Title", task.Title);
						cmd.Parameters.AddWithValue("@Description",
							string.IsNullOrWhiteSpace(task.Description) ? (object)DBNull.Value : task.Description);

						cmd.ExecuteNonQuery();
					}
				}
			}

			Console.WriteLine($"{tasks.Count} task(s) added successfully.");
		}

		// ============ SHOW TASKS ============
		static void ShowTasks()
		{
			Console.WriteLine("\n===== TASKS =====");

			using (SqlConnection conn = new SqlConnection(ConnectionString))
			{
				conn.Open();

				string sql = "SELECT Id, TaskTitle, Description, IsDone FROM Tasks ORDER BY Id;";

				using (SqlCommand cmd = new SqlCommand(sql, conn))
				using (SqlDataReader reader = cmd.ExecuteReader())
				{
					if (!reader.HasRows)
					{
						Console.WriteLine("No tasks found.");
						return;
					}

					while (reader.Read())
					{
						int id = reader.GetInt32(0);
						string title = reader.GetString(1);
						string description = reader.IsDBNull(2) ? "" : reader.GetString(2);
						bool IsDone = reader.GetBoolean(3);

						string status = IsDone ? "[DONE]" : "[TODO]";

						if (!string.IsNullOrWhiteSpace(description))
							Console.WriteLine($"{id}. {status} {title} - {description}");
						else
							Console.WriteLine($"{id}. {status} {title}");
					}
				}
			}
		}

		// ============ MARK TASK AS DONE ============
		static void MarkTaskAsDone()
		{
			Console.Write("\nEnter task Id to mark as done: ");
			string input = Console.ReadLine();

			if (!int.TryParse(input, out int id))
			{
				Console.WriteLine("Invalid Id.");
				return;
			}

			using (SqlConnection conn = new SqlConnection(ConnectionString))
			{
				conn.Open();

				string sql = "UPDATE Tasks SET IsDone = 1 WHERE Id = @Id;";

				using (SqlCommand cmd = new SqlCommand(sql, conn))
				{
					cmd.Parameters.AddWithValue("@Id", id);

					int rows = cmd.ExecuteNonQuery();

					if (rows > 0)
						Console.WriteLine("Task marked as done.");
					else
						Console.WriteLine("Task not found.");
				}
			}
		}

		// ============ DELETE TASK ============
		static void DeleteTask()
		{
			Console.Write("\nEnter task Id to delete: ");
			string input = Console.ReadLine();

			if (!int.TryParse(input, out int id))
			{
				Console.WriteLine("Invalid Id.");
				return;
			}

			using (SqlConnection conn = new SqlConnection(ConnectionString))
			{
				conn.Open();

				string sql = "DELETE FROM Tasks WHERE Id = @Id;";

				using (SqlCommand cmd = new SqlCommand(sql, conn))
				{
					cmd.Parameters.AddWithValue("@Id", id);

					int rows = cmd.ExecuteNonQuery();

					if (rows > 0)
						Console.WriteLine("Task deleted.");
					else
						Console.WriteLine("Task not found.");
				}
			}
		}
	}
}
