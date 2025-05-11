using System;

namespace ID2.ArchipelagoRandomizer;

class LoginValidationException(string message) : Exception(message)
{
}