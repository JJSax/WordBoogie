
import msvcrt
from enum import Enum

print("Begin reducing words.")
print("Remove x Word?")
print("Press 'n' to keep a word in the list.")
print("Press 'y' to DELETE a word in the list. It will be saved in a separate file in case of mistakes.")
print("press 'e' to escape.")

class PromptResponse(Enum):
	Keep = 1
	Delete = 2
	Break = 3

def promptWord(word) -> PromptResponse:
	while True:
		print("Delete \"" + word.rstrip() + "\"?")
		c = msvcrt.getch()
		char = c.decode().lower()
		print("you entered" , char)
		if char == 'n':
			return PromptResponse.Keep
		elif char == 'y':
			return PromptResponse.Delete
		elif char == 'e':
			return PromptResponse.Break
		else:
			pass

wordsToMove = []

def appendToFile(file, line):
	with open(file, "a") as f:
		f.write(line)

def deleteWord(word, index):
	print("Deleting " + word + " at index: " + str(index))
	wordsToMove.append(index)

def breakout():
	print("Halting Operations and Removing words")

	# loop through sorted wordsToMove in reverse to avoid index shifting
	# remove that index from allWords and put it in backup file just in case.
	for i in reversed(wordsToMove):
		word = allWords.pop(i)
		appendToFile("removedWords.txt", word) # backup file

	# Loop through remaining allWords to replace file
	with open("words_alpha_jj.txt", "w") as f:
		for items in allWords:
			f.write(items)

	print("Files written to successfully")

def wordIsOfNLen(word, n):
	return len(word) == n

def wordAt(index):
	return allWords[index].rstrip()


if __name__ == "__main__":
	allWords = []
	#! Will need to change from confirmed words
	with open("words_alpha_jj.txt", "r") as f:
		allWords = f.readlines()

	currentIndex = 0
	shouldStop = False
	while True:
		while True:
			# finding a word I want to check / Filter words I don't want to check
			if len(wordAt(currentIndex)) == 3:
				break # break out of loop searching for a word I want to check

			currentIndex += 1
			if currentIndex >= len(allWords): # if you get to end of full list, take out selection
				breakout()
				shouldStop = True
				break

		if shouldStop:
			break

		response = promptWord(wordAt(currentIndex))
		if response == PromptResponse.Break:
			breakout()
			break
		elif response == PromptResponse.Keep:
			pass
		elif response == PromptResponse.Delete:
			deleteWord(wordAt(currentIndex), currentIndex)
		else:
			print("Something went wrong.")

		currentIndex += 1