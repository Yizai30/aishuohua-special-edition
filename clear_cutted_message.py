import os
import string
files = os.listdir("TestCases")
for f in files:
    result = ''.join([c for c in open(os.path.join("TestCases", f),
                                      "r", encoding="utf-8").read() if not c in string.ascii_lowercase])
    open(os.path.join("TestCases", f), "w", encoding="utf-8").write(result)
