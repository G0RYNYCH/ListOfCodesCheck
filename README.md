ListOfCodesCheck

This is a console application.
The input is a file with a list of codes (each code on a new line) and check line. Arguments are passed through the cmd.

The output is a new file which formed with the same codes, but after the TAB character, the result of checking the code is displayed:
   * OK - no errors
   * Error text

Examples:
   * Check Line: 1234 - 7, 123 - 5+, 12 - 3, 234 - 3+
   * List of codes:
      1234123456712312345\u001d12123234123\u001d23156   Error
      1234123456712312345\u001d12123234                 Error
      1234123456712312345\u001d12123234123              OK
      1234123456712312345\u001d121232341231235468       OK
      1234123456712312345\u001d1212323412               Error
      1234123456712312345\u001d12123234                 Error
