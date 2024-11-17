import os
import shutil
from datetime import datetime

def get_files_in_directory(directory):
    """Retrieve a list of files in the specified directory (excluding subdirectories)."""
    return [f for f in os.listdir(directory) if os.path.isfile(os.path.join(directory, f))]

def sort_files(files, directory, sort_option):
    """Sort files based on the selected option."""
    if sort_option == 1:
        return sorted(files)  # Sort by filename (alphabetically)
    elif sort_option == 2:
        return sorted(files, key=lambda f: os.path.getsize(os.path.join(directory, f)))  # Sort by filesize
    elif sort_option == 3:
        return sorted(files, key=lambda f: os.path.getmtime(os.path.join(directory, f)))  # Date modified ascending
    elif sort_option == 4:
        return sorted(files, key=lambda f: os.path.getmtime(os.path.join(directory, f)), reverse=True)  # Date modified descending
    elif sort_option == 5:
        return sorted(files, key=lambda f: os.path.splitext(f)[1])   # Use extension for sorting

    return files

def categorize_files(files, directory, sort_option):
    """Categorize files into folders based on the selected sorting criterion."""
    if sort_option == 1:
        # Categorize by filename (e.g., A-files, B-files, etc.)
        for file in files:
            first_letter = file[0].upper()
            folder = os.path.join(directory, f"{first_letter}_files")
            os.makedirs(folder, exist_ok=True)
            shutil.move(os.path.join(directory, file), os.path.join(folder, file))

    elif sort_option == 2:
        # Categorize by filesize (e.g., Small, Medium, Large)
        for file in files:
            size = os.path.getsize(os.path.join(directory, file))
            if size < 1_000_000:  # Less than 1MB
                folder = os.path.join(directory, "Small_files")
            elif size < 10_000_000:  # Less than 10MB
                folder = os.path.join(directory, "Medium_files")
            else:
                folder = os.path.join(directory, "Large_files")
            os.makedirs(folder, exist_ok=True)
            shutil.move(os.path.join(directory, file), os.path.join(folder, file))

    elif sort_option in (3, 4):
        # Categorize by modification date (e.g., Year-Month)
        for file in files:
            mod_time = os.path.getmtime(os.path.join(directory, file))
            date = datetime.fromtimestamp(mod_time).strftime('%Y-%m')
            folder = os.path.join(directory, date)
            os.makedirs(folder, exist_ok=True)
            shutil.move(os.path.join(directory, file), os.path.join(folder, file))

    elif sort_option == 5:
        # Categorize by file extension (e.g., .txt, .jpg, etc.)
        for file in files:
            ext = os.path.splitext(file)[1][1:]  # Get extension without dot
            folder = os.path.join(directory, ext + "_files")
            os.makedirs(folder, exist_ok=True)
            shutil.move(os.path.join(directory, file), os.path.join(folder, file))

def main():
    directory = input("Enter the directory path: ")
    
    if not os.path.isdir(directory):
        print("Invalid directory path.")
        return
    
    print("Choose a sorting option:")
    print("1 - Filename")
    print("2 - Filesize")
    print("3 - Date modified (ascending)")
    print("4 - Date modified (descending)")
    print("5 - Extension")
    
    try:
        sort_option = int(input("Enter your choice (1-5): "))
        if sort_option not in [1, 2, 3, 4, 5]:
            raise ValueError
    except ValueError:
        print("Invalid choice. Please enter a number between 1 and 5.")
        return
    
    files = get_files_in_directory(directory)
    sorted_files = sort_files(files, directory, sort_option)
    
    print("Sorted files:")
    for file in sorted_files:
        print(file)

    categorize = input("Would you like to categorize files into separate folders based on the chosen sorting criterion? (yes/no): ").strip().lower()
    
    if categorize == 'yes':
        categorize_files(sorted_files, directory, sort_option)
        print("Files have been categorized into separate folders.")
    else:
        print("Categorization skipped.")

if __name__ == "__main__":
    while(True):
        main()
        try_again = input("Do you want to continue organizing files? (yes/no): ")
        if try_again != "yes":
            break
    
