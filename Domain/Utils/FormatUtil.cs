using Domain.Entities;
using Domain.Entities.DTOs;
using Newtonsoft.Json;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Domain.Utils
{
    public static class FormatUtil
    {
        public static int GetIdEntity(object entity)
        {
            var sourceProperties = entity.GetType().GetProperties();
            var entityProperty = sourceProperties.FirstOrDefault(x => x.Name == "Id");
            if (entityProperty != null)
            {
                var value = entityProperty.GetValue(entity);
                if (value != null)
                {
                    return (int)value;
                }
            }
            return 0;
        }

        public static string GenerateUniqueFileName(string fileName)
        {
            // Get the file extension
            string fileExtension = Path.GetExtension(fileName);

            // Generate a random string
            string uniqueFilename = $"{Guid.NewGuid()}_{DateTime.Now.Ticks}{fileExtension}";

            // Return the file name with the random string and the original file extension
            return uniqueFilename;
        }

        public static void TrimObjectProperties(object obj)
        {
            if (obj == null) return;

            var type = obj.GetType();
            var properties = type.GetProperties().Where(p => p.PropertyType == typeof(string));
            foreach (var property in properties)
            {
                var value = (string)property.GetValue(obj);
                if (value != null)
                {
                    property.SetValue(obj, value.Trim());
                }
            }
        }
        public static void SetIsActive<T>(T data, bool isActive) where T : BaseEntity
        {
            Type type = typeof(T);
            PropertyInfo lastUpdateByProperty = type.GetProperty("IsActive");
            if (lastUpdateByProperty != null)
            {
                lastUpdateByProperty.SetValue(data, isActive);
            }
        }
        public static void SetDateBaseEntity<T>(T data, bool isUpdate =false)where T : BaseEntity
        {
            data.UpdatedAt = DateTime.Now;
            if (!isUpdate)
            {
                data.CreatedAt = DateTime.Now;
            }
        }
        public static void SetOppositeActive<T>(T data) where T : class
        {
            Type type = typeof(T);
            PropertyInfo lastUpdateByProperty = type.GetProperty("IsActive");
            if (lastUpdateByProperty != null)
            {
                bool currentValue = (bool)lastUpdateByProperty.GetValue(data);
                lastUpdateByProperty.SetValue(data, !currentValue);
            }
        }

        public static List<ImageProp> HasImageProperty(object obj)
        {
            var sourceProperties = obj.GetType().GetProperties();
            var imageProperty = sourceProperties.Where(x => x.Name == "Image" || x.Name.Contains("Picture")).ToList();
            var data = new List<ImageProp>();
            if (imageProperty != null)
            {
                foreach (var property in imageProperty)
                {
                    var imageValue = property.GetValue(obj);
                    var imageType = property.Name;
                    if (imageValue != null)
                    {
                        var imageProp = new ImageProp()
                        {
                            Name = imageType,
                            Image = imageValue.ToString()
                        };
                        data.Add(imageProp);
                    }
                }
            }
            return data;
        }

        public static void ConvertUpdateObject<TSource, TDestination>(TSource source, TDestination destination)
            where TSource : class
            where TDestination : class
        {
            var sourceProperties = source.GetType().GetProperties();
            var destinationProperties = destination.GetType().GetProperties();

            foreach (var sourceProperty in sourceProperties)
            {
                if (sourceProperty.Name.Contains("Id") && sourceProperty.Name != "Id")
                {
                    // Check if the value is 0 or null and continue if necessary
                    if (sourceProperty.GetValue(source) == null || (int)sourceProperty.GetValue(source) == 0)
                    {
                        continue;
                    }
                }
                if (sourceProperty.Name == "Id")
                {
                    var destinationProperty2 = destinationProperties.FirstOrDefault(x => x.Name == sourceProperty.Name);
                    if (destinationProperty2 != null)
                    {
                        sourceProperty.SetValue(source, destinationProperty2.GetValue(destination));
                    }
                    continue;
                }
                if (sourceProperty.Name == "Longitude" || sourceProperty.Name == "Latitude")
                {
                    // Check if the value is 0 or null and continue if necessary
                    if (sourceProperty.GetValue(source) == null || (double)sourceProperty.GetValue(source) == 0)
                    {
                        continue;
                    }
                }
                if (sourceProperty.Name == "CreatedAt") continue;
                if (sourceProperty.Name == "UpdatedAt") continue;
                if (sourceProperty.GetValue(source) == null) continue;

                var destinationProperty = destinationProperties.FirstOrDefault(x => x.Name == sourceProperty.Name);
                if (destinationProperty != null)
                {
                    destinationProperty.SetValue(destination, sourceProperty.GetValue(source));
                }
            }
        }
        public static bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false;
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidPassword(string password)
        {
            if (password != string.Empty && password != null)
            {
                if (!Regex.IsMatch(password, "^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.{8,})")) return false;

                var specialchar = new List<string> { "@", "#", "$", "%", "^", "&", "+", "=", ".", ",", "<", ">", "`", "!", "/", "?", "@", "\"", "'", "~", "\\", "[", "]", "{", "}", "*", "(", ")", "-", "+", "|", "_", "[", "]", "/", ":", ";" };
                if (!specialchar.Any(a => password.Contains(a))) return false;

                return true;
            }
            else return false;
        }

        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string DeleteSerializeTempData<T>(List<T> temp, T deletedData)
        {
            temp.Remove(deletedData);
            return JsonConvert.SerializeObject(temp);
        }
        public static string InsertSerializeTempData<T>(List<T> temp, T newData)
        {
            if (temp == null)
            {
                var list = new List<T>();
                list.Add(newData);
                return JsonConvert.SerializeObject(list);
            }
            temp.Add(newData);
            return JsonConvert.SerializeObject(temp);
        }
        public static List<T> DeserializeTempData<T>(string data)
        {
            if (string.IsNullOrEmpty(data)) return default(List<T>);
            var list = JsonConvert.DeserializeObject<List<T>>(data);
            return list;
        }

        public static string ReplaceSpacesWithUnderscores(string input)
        {
            return input.Replace(" ", "_");
        }
    }
}
