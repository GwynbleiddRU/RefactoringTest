using System;

namespace LegacyApp
{
    public class UserService
    {
        private readonly ClientRepository _clientRepository;
        private readonly IUserCreditService _userCreditService;

        public UserService(ClientRepository clientRepository, IUserCreditService userCreditService)
        {
            _clientRepository = clientRepository;
            _userCreditService = userCreditService;
        }

        public bool AddUser(
            string firstName,
            string surname,
            string email,
            DateTime dateOfBirth,
            int clientId
        )
        {
            if (!IsValidUserInput(firstName, surname, email, dateOfBirth))
            {
                return false;
            }

            var client = _clientRepository.GetById(clientId);
            var user = CreateUser(firstName, surname, email, dateOfBirth, client);

            SetCreditLimit(user, client);

            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }

            UserDataAccess.AddUser(user);
            return true;
        }

        private bool IsValidUserInput(
            string firstName,
            string surname,
            string email,
            DateTime dateOfBirth
        )
        {
            return !string.IsNullOrEmpty(firstName)
                && !string.IsNullOrEmpty(surname)
                && email.Contains("@")
                && email.Contains(".")
                && CalculateAge(dateOfBirth) >= 21;
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (
                now.Month < dateOfBirth.Month
                || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)
            )
            {
                age--;
            }
            return age;
        }

        private User CreateUser(
            string firstName,
            string surname,
            string email,
            DateTime dateOfBirth,
            Client client
        )
        {
            return new User
            {
                FirstName = firstName,
                Surname = surname,
                EmailAddress = email,
                DateOfBirth = dateOfBirth,
                Client = client
            };
        }

        private void SetCreditLimit(User user, Client client)
        {
            if (client.Name == "VeryImportantClient")
            {
                user.HasCreditLimit = false;
            }
            else
            {
                user.HasCreditLimit = true;
                int creditLimit = _userCreditService.GetCreditLimit(
                    user.FirstName,
                    user.Surname,
                    user.DateOfBirth
                );
                user.CreditLimit = client.Name == "ImportantClient" ? creditLimit * 2 : creditLimit;
            }
        }
    }
}
