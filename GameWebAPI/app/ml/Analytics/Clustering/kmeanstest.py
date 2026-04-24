import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
import os, joblib

from sklearn.cluster import KMeans
from sklearn.preprocessing import StandardScaler
from sklearn.decomposition import PCA
from sklearn.metrics import silhouette_score

# =========================================================
# 1. LOAD REAL DATASET
# =========================================================
# Change this path if needed
csv_path = r"C:\UnityProjects\FYP\Analytics\Preprocessed\session_features.csv"

pd.set_option("display.max_columns", None)
pd.set_option("display.width", None)
pd.set_option("display.max_rows", None)
pd.set_option("display.max_colwidth", None)


df = pd.read_csv(csv_path)
# Assume last 3 rows are your test runs
# df = df[df["session_id"] != "20260406_202420"]

# Optional: save a backup copy / inspect later
# df.to_csv("loaded_session_features_copy.csv", index=False)

# =========================================================
# 2. VISUALIZATION / BASIC INSPECTION
# =========================================================
print(df.shape)
print("-" * 50)
print(df.info())
print("-" * 50)
print(df.describe())
print("-" * 50)
print(df.describe(include=["O"]))
print("-" * 50)
print(df.head())

# =========================================================
# 3. DATA CLEANING
# =========================================================
duplicate_rows = df[df.duplicated()]
print(f"Number of duplicate rows before dropping: {duplicate_rows.shape}")

df = df.drop_duplicates()

duplicate_rows = df[df.duplicated()]
print(f"Number of duplicate rows after dropping: {duplicate_rows.shape}")

print("-" * 50)
print("Null values:\n", df.isnull().sum())

print("-" * 50)
print(f"Dataset shape after cleaning: {df.shape}")

# =========================================================
# 4. OPTIONAL OUTLIER REMOVAL USING IQR
# =========================================================
# We exclude non-numeric columns like session_id from this step.

# df_train = df.copy()
df_train = df.iloc[:-1].copy()
df_test = df.iloc[-1:].copy()

numeric_cols = df.select_dtypes(include=[np.number]).columns

Q1 = df[numeric_cols].quantile(0.25)
Q3 = df[numeric_cols].quantile(0.75)
IQR = Q3 - Q1

# df_clean = df[
#     ~((df[numeric_cols] < (Q1 - 1.5 * IQR)) | (df[numeric_cols] > (Q3 + 1.5 * IQR))).any(axis=1)
# ].copy()

df_clean = df_train.copy()

print("-" * 50)
print(f"Shape after IQR outlier removal: {df_clean.shape}")

# fallback: if IQR removes too many rows, use original cleaned df
if len(df_clean) < 3:
    print("[WARN] Too few rows after IQR filtering. Reverting to non-IQR-cleaned dataset.")
    df_clean = df_train.copy()

# =========================================================
# 5. CORRELATION ANALYSIS
# =========================================================
corr_numeric_cols = df_clean.select_dtypes(include=[np.number]).columns
corr = df_clean[corr_numeric_cols].corr(method="pearson")
# keep only columns that have at least one strong correlation
mask = (corr.abs() > 0.5)

cols_to_keep = mask.any(axis=0)
corr_filtered = corr.loc[cols_to_keep, cols_to_keep]

plt.figure(figsize=(14, 10))
sns.heatmap(
    corr,
    vmin=-1,
    vmax=1,
    cmap="coolwarm",
    annot=True,
    linewidths=0.1
)
plt.title("Pearson Correlation Heatmap")
plt.tight_layout()
plt.show()

# =========================================================
# 6. PREPARE DATA FOR CLUSTERING
# =========================================================
feature_cols = [
    "avg_house_per_turn",
    # "avg_factory_per_turn",
    # "avg_service_per_turn",
    "service_to_house",
    "factory_to_house",

    # "avg_satisfaction_index",
    # "avg_pollution_index",
    # "avg_service_index",

    "avg_population_growth",
    "avg_gold_growth",
    "avg_supply_ratio",
    # "min_supply_ratio",

    #early game feats
    "used_service_early",
    "used_factory_early",
    # "early_service_ratio",
    # "early_factory_ratio",

    "avg_action_intensity"
]

corr_matrix = df[feature_cols].corr(numeric_only=True)

import seaborn as sns
import matplotlib.pyplot as plt

plt.figure(figsize=(10, 8))
sns.heatmap(
    corr_matrix,
    annot=True,
    fmt=".2f",
    cmap="coolwarm",
    vmin=-1,
    vmax=1,
    linewidths=0.5
)
plt.title("Feature-to-Feature Correlation Matrix")
plt.tight_layout()
plt.show()

missing_feature_cols = [col for col in feature_cols if col not in df_clean.columns]
if missing_feature_cols:
    raise ValueError(f"Missing required feature columns: {missing_feature_cols}")

X = df_clean[feature_cols].copy()

# fill numeric NaN just in case
X = X.fillna(0)

# Standardize features because K-means is distance-based
scaler = StandardScaler()
X_scaled = scaler.fit_transform(X)

print("\nPrepared feature matrix shape:", X_scaled.shape)

# =========================================================
# 7. FINDING THE BEST K VALUE
# =========================================================
# K must be less than number of samples
max_k = min(6, len(df_clean) - 1)

if max_k < 2:
    raise ValueError("Not enough rows to perform clustering. Need at least 3 sessions.")

k_values = range(2, max_k + 1)
inertia_scores = []
silhouette_scores = []

for k in k_values:
    kmeans = KMeans(n_clusters=k, random_state=42, n_init=10)
    cluster_labels = kmeans.fit_predict(X_scaled)

    inertia_scores.append(kmeans.inertia_)
    silhouette_scores.append(silhouette_score(X_scaled, cluster_labels))

# Plot Elbow
plt.figure(figsize=(10, 6))
plt.plot(list(k_values), inertia_scores, marker="o", linestyle="dashed")
plt.xlabel("K value")
plt.ylabel("Inertia")
plt.title("Elbow Method for K-means")
plt.tight_layout()
plt.show()

# Plot Silhouette
plt.figure(figsize=(10, 6))
plt.plot(list(k_values), silhouette_scores, marker="o", linestyle="dashed")
plt.xlabel("K value")
plt.ylabel("Silhouette Score")
plt.title("Silhouette Score vs K value")
plt.tight_layout()
plt.show()

best_k = list(k_values)[np.argmax(silhouette_scores)]
print(f"Best K based on silhouette score: {best_k}")

# =========================================================
# 8. TRAIN FINAL K-MEANS MODEL
# =========================================================
kmeans = KMeans(n_clusters=best_k, random_state=42, n_init=10)
df_clean["cluster"] = kmeans.fit_predict(X_scaled)

print("-" * 50)
print(df_clean[["session_id", "cluster"]].head(20))

# =========================================================
# 9. CLUSTER SUMMARY / INTERPRETATION
# =========================================================
cluster_summary = df_clean.groupby("cluster")[feature_cols].mean()

print("-" * 50)
print("Cluster summary:")
print(cluster_summary)

print("-" * 50)
print("Cluster counts:")
print(df_clean["cluster"].value_counts().sort_index())

# =========================================================
# 10. VISUALIZATION OF CLUSTERS USING PCA
# =========================================================
pca = PCA(n_components=2)
X_pca = pca.fit_transform(X_scaled)

plot_df = pd.DataFrame({
    "PCA1": X_pca[:, 0],
    "PCA2": X_pca[:, 1],
    "cluster": df_clean["cluster"].values,
    "session_id": df_clean["session_id"].values if "session_id" in df_clean.columns else np.arange(len(df_clean))
})

plt.figure(figsize=(10, 7))
sns.scatterplot(data=plot_df, x="PCA1", y="PCA2", hue="cluster", s=100)

for _, row in plot_df.iterrows():
    plt.text(row["PCA1"], row["PCA2"], str(row["session_id"]), fontsize=8)

plt.title("K-means Clusters Visualized with PCA")
plt.tight_layout()
plt.show()

# =========================================================
# 11. CLUSTER CENTERS IN ORIGINAL SCALE
# =========================================================
centers_scaled = kmeans.cluster_centers_
centers_original = scaler.inverse_transform(centers_scaled)
centers_df = pd.DataFrame(centers_original, columns=feature_cols)

print("-" * 50)
print("Cluster centers in original feature scale:")
print(centers_df)

# =========================================================
# 12. OPTIONAL MANUAL MAPPING OF CLUSTER -> STYLE NAME
# =========================================================
cluster_name_map = {
    0: "Unknown",
    1: "Unknown",
    2: "Unknown",
    3: "Unknown",
    4: "Unknown",
    5: "Unknown",
    6: "Unknown",
    7: "Unknown",
}

df_clean["cluster_style_name"] = df_clean["cluster"].map(
    lambda x: cluster_name_map.get(x, "Unknown")
)

print("-" * 50)
print(df_clean[["session_id", "cluster", "cluster_style_name"]])

# =========================================================
# 13. OPTIONAL SAVE OUTPUTS
# =========================================================
df_clean.to_csv("clustered_session_features.csv", index=False)
cluster_summary.to_csv("cluster_summary.csv")
centers_df.to_csv("cluster_centers.csv", index=False)

print("-" * 50)
print("Saved:")
print("- clustered_session_features.csv")
print("- cluster_summary.csv")
print("- cluster_centers.csv")

# =========================
# TEST ON LAST 3 RUNS
# =========================
X_test = df_test[feature_cols].copy()
X_test = X_test.fillna(0)

# use SAME scaler
X_test_scaled = scaler.transform(X_test)

# predict cluster
test_clusters = kmeans.predict(X_test_scaled)

df_test["predicted_cluster"] = test_clusters

print("-" * 50)
print("Test runs prediction:")
print(df_test[["session_id", "predicted_cluster"]])

# save model
save_dir = r"C:\UnityProjects\FYP\GameWebAPI\app\ml\TrainedModels\Clustering"
os.makedirs(save_dir, exist_ok=True)

joblib.dump(kmeans, os.path.join(save_dir, "kmeans_model.pkl"))
joblib.dump(scaler, os.path.join(save_dir, "scaler.pkl"))
joblib.dump(feature_cols, os.path.join(save_dir, "feature_cols.pkl"))

print("Model, scaler, and feature list saved.")