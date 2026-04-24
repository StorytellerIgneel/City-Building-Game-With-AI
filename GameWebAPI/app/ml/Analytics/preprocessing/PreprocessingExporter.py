import os
import csv

def export_session_features(session_features, folder_path="Analytics\\Clustering", file_name="session_features.csv"):
    # Ensure folder exists
    os.makedirs(folder_path, exist_ok=True)

    file_path = os.path.join(folder_path, file_name)

    # Check if file already exists (to decide header)
    file_exists = os.path.isfile(file_path)

    # Extract headers from dict keys
    headers = list(session_features.keys())

    try:
        with open(file_path, mode="a", newline="", encoding="utf-8") as file:
            writer = csv.DictWriter(file, fieldnames=headers)

            # Write header only if file is new
            if not file_exists:
                writer.writeheader()

            writer.writerow(session_features)

        print(f"[SUCCESS] Session exported to {file_path}")

    except Exception as e:
        print(f"[ERROR] Failed to export session: {e}")