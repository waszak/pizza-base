using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Baza_pizzerii
{
    static class Validate
    {

        public static bool Password(string pass, out string msg)
        {
            if (pass.Length < 6)
            {
                msg = "Hasło musi składać się przynajmniej z 6 znaków!";
                return false;
            }

            if (!Regex.IsMatch(pass, @"^[a-zA-Z0-9]*$"))
            {
                msg = "Hasło zawiera niepoprawne znaki! \nPoprawne hasło składa się wyłącznie z liter i cyfr.";
                return false;
            }

            msg = "";
            return true;
        }

        public static bool Passwords(string pass1, string pass2, out string msg)
        {
            if (pass1.Length < 6)
            {
                msg = "Hasło musi składać się przynajmniej z 6 znaków!";
                return false;
            }

            if (!Regex.IsMatch(pass1, @"^[a-zA-Z0-9]*$"))
            {
                msg = "Hasło zawiera niepoprawne znaki! \nPoprawne hasło składa się wyłącznie z liter i cyfr.";
                return false;
            }

            if (pass1 != pass2)
            {
                msg = "Hasła są różne!";
                return false;
            }
            msg = "";
            return true;
        }

        public static bool OnlyAlphaNumeric(string text, string fieldname, out string msg)
        {
            if (!Regex.IsMatch(text, @"^[a-zA-Z0-9]*$"))
            {
                msg = fieldname + " zawiera niepoprawne znaki! \nDozwolone są wyłącznie litery i cyfry.";
                return false;
            }
            msg = "";
            return true;
        }

        public static bool OnlyLetters(string text, string fieldname, out string msg)
        {
            if (!Regex.IsMatch(text, @"^[a-zA-Z]*$"))
            {
                msg = fieldname + " zawiera niepoprawne znaki! \nDozwolone są wyłącznie litery.";
                return false;
            }
            msg = "";
            return true;
        }

        public static bool Login(string login, out string msg)
        {
            if (login.Length == 0)
            {
                msg = "login nie może być pusty!";
                return false;
            }

            if (!Regex.IsMatch(login, @"^[a-zA-Z][a-zA-Z0-9]*$"))
            {
                msg = "Login zawiera niepoprawne znaki! \nW poprawnym loginie pierwszy znak jest literą, a reszta znaków literą lub cyfrą.";
                return false;
            }
            msg = "";
            return true;
        }

        public static bool Email(string email, out string msg)
        {
            if (email.Length == 0)
            {
                msg = "E-mail nie może być pusty!";
                return false;
            }

            if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._]+@[a-zA-Z0-9._]+$"))
            {
                msg = "E-mail jest niepoprawny!";
                return false;
            }

            msg = "";
            return true;
        }

        public static bool Address(string address, out string msg)
        {
            if (address.Length == 0)
            {
                msg = "Adres nie może być pusty!";
                return false;
            }

            if (!Regex.IsMatch(address, @"^[a-zA-Z0-9.,-/ ]*$")) {
                msg = "Adres jest niepoprawny!\nPowinien składać się wyłącznie z liter, cyfr i znaków - / . ,";
                return false;
            }

            msg = "";
            return true;
        }

        public static bool PhoneNumber(string number, out string msg)
        {
            if (number.Length == 0)
            {
                msg = "Numer nie może być pusty!";
                return false;
            }
        
            if (!Regex.IsMatch(number, @"^[0-9-]*$")) {
                msg = "Numer telefonu ma niepoprawny format! \nPowinien składać się wyłącznie z cyfr i '-'";
                return false;
            }

            msg = "";
            return true;
        }

    }
}
