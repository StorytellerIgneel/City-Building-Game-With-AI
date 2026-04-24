import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
import joblib
import os

from sklearn.linear_model import LinearRegression
from sklearn.metrics import mean_squared_error, r2_score

# =========================================================
# 1. READ CSV
# =========================================================

csv_path = r"C:\UnityProjects\FYP\Analytics\Preprocessed\session_features.csv"

pd.set_option("display.max_columns", None)
pd.set_option("display.width", None)
pd.set_option("display.max_rows", None)
pd.set_option("display.max_colwidth", None)

df = pd.read_csv(csv_path)

print("-" * 50)
print("Original dataset shape:", df.shape)
print(df.head())

# =========================================================
# 2. TRAIN / TEST SPLIT
#    last row = current session to predict
# =========================================================

df_train = df.copy()
# df_train = df.iloc[:-1].copy()
# df_test = df.iloc[-1:].copy()

print("-" * 50)
print("Train shape before cleaning:", df_train.shape)
# print("Test shape:", df_test.shape)

# =========================================================
# 3. BASIC CLEANING
# =========================================================

# remove duplicated rows if any
df_train = df_train.drop_duplicates().copy()

# replace inf with nan
df_train = df_train.replace([np.inf, -np.inf], np.nan)
# df_test = df_test.replace([np.inf, -np.inf], np.nan)

# =========================================================
# 4. OPTIONAL IQR OUTLIER REMOVAL
#    currently kept simple; can disable if too aggressive
# =========================================================

numeric_cols = df_train.select_dtypes(include=[np.number]).columns

Q1 = df_train[numeric_cols].quantile(0.25)
Q3 = df_train[numeric_cols].quantile(0.75)
IQR = Q3 - Q1

# Uncomment this if you really want IQR filtering:
# df_clean = df_train[
#     ~((df_train[numeric_cols] < (Q1 - 1.5 * IQR)) |
#       (df_train[numeric_cols] > (Q3 + 1.5 * IQR))).any(axis=1)
# ].copy()

# for now, use full training set
df_clean = df_train.copy()

print("-" * 50)
print(f"Shape after cleaning: {df_clean.shape}")

if len(df_clean) < 3:
    print("[WARN] Too few rows after cleaning. Reverting to original training data.")
    df_clean = df_train.copy()

# =========================================================
# 5. FEATURE / TARGET SELECTION
# =========================================================

# target_col = "final_population"

# feature_cols = [
#     "Population",
#     "Gold",
#     "TotalSupplyProvided",
#     "AP",
#     "SmallHouseCount",
#     "BigHouseCount",
#     "SupplyCount",
#     "ServiceCount",
#     "FactoryCount",
#     "RoadCount",
#     "AverageSatisfactionIndex",
#     "AveragePollutionIndex",
#     "AverageServiceIndex",
#     "HousesNearFactoryCount",
#     "HousesWithoutServiceCount",
#     "TotalTaxIncome"
# ]

# # keep only needed columns and drop rows with missing values in training
# df_clean = df_clean.dropna(subset=feature_cols + [target_col]).copy()

# # for test row, fill missing with train medians if needed
# for col in feature_cols:
#     if df_test[col].isna().any():
#         df_test[col] = df_test[col].fillna(df_clean[col].median())

# X_train = df_clean[feature_cols]
# y_train = df_clean[target_col]

# X_test = df_test[feature_cols]
# y_test = df_test[target_col]

# print("-" * 50)
# print(f"Train shape: {X_train.shape}")
# print(f"Test shape: {X_test.shape}")

from sklearn.model_selection import train_test_split

# =========================================================
# SPLIT DATA (proper test_size)
# =========================================================

target_col = "final_population"

feature_cols = [
    "avg_population_growth",
    "avg_gold_growth",
    "avg_supply_growth",
    "avg_ap_growth",
    "avg_satisfaction_growth",
    "avg_service_growth",
    "avg_pollution_growth",

    "avg_satisfaction_index",
    "avg_service_index",
    "avg_pollution_index",

    "avg_supply_ratio",
    "min_supply_ratio",
    "supply_efficiency",

    "avg_house_per_turn",
    "avg_factory_per_turn",
    "avg_service_per_turn",
    "avg_supply_per_turn",

    "avg_no_service",
    "avg_low_satisfaction",

    "early_service_ratio",
    "early_factory_ratio"
]

# remove missing
df_model = df_clean.dropna(subset=feature_cols + [target_col]).copy()

X = df_model[feature_cols]
y = df_model[target_col]

# 80% train, 20% test
X_train, X_test, y_train, y_test = train_test_split(
    X, y,
    test_size=0.2,
    random_state=42
)

print("-" * 50)
print(f"Train shape: {X_train.shape}")
print(f"Test shape: {X_test.shape}")

# =========================================================
# 6. CORRELATION CHECK FOR SELECTED FEATURES
# =========================================================

df_corr = df_clean[feature_cols + [target_col]].copy()
corr_with_target = df_corr.corr(numeric_only=True)[target_col].drop(target_col)
corr_sorted = corr_with_target.reindex(
    corr_with_target.abs().sort_values(ascending=False).index
)

print("-" * 50)
print(f"Correlation with {target_col}:")
print(corr_sorted)

plt.figure(figsize=(10, 6))
corr_sorted.plot(kind="bar")
plt.title(f"Feature Correlation with {target_col}")
plt.ylabel("Correlation")
plt.xticks(rotation=45, ha="right")
plt.tight_layout()
plt.show()

# optional feature-vs-feature heatmap
plt.figure(figsize=(12, 8))
sns.heatmap(
    df_clean[feature_cols + [target_col]].corr(numeric_only=True),
    vmin=-1,
    vmax=1,
    cmap="coolwarm",
    annot=True,
    linewidths=0.1
)
plt.title("Correlation Heatmap")
plt.tight_layout()
plt.show()

# =========================================================
# 7. TRAIN MODEL
# =========================================================

print("-" * 50)
print("Starting Linear Regression...")

regressor = LinearRegression()
regressor.fit(X_train, y_train)

# =========================================================
# 8. PREDICTION
# =========================================================

y_pred = regressor.predict(X_test)

# =========================================================
# 9. EVALUATION
# =========================================================

mse = mean_squared_error(y_test, y_pred)

print("-" * 50)
print("Linear Regression Results")
print(f"Predicted Population: {y_pred[0]:.2f}")
print(f"Actual Population: {y_test.values[0]:.2f}")
print(f"MSE: {mse:.2f}")

# =========================================================
# 10. COEFFICIENT ANALYSIS
# =========================================================

coeff_df = pd.DataFrame({
    "Feature": feature_cols,
    "Coefficient": regressor.coef_
}).sort_values(by="Coefficient", key=np.abs, ascending=False)

print("-" * 50)
print("Top Influential Features:")
print(coeff_df)

# =========================================================
# 11. TRAINING PERFORMANCE
# =========================================================

y_train_pred = regressor.predict(X_train)

mse_train = mean_squared_error(y_train, y_train_pred)
r2_train = r2_score(y_train, y_train_pred)

print("-" * 50)
print("Training Performance:")
print(f"MSE (train): {mse_train:.2f}")
print(f"R2 (train): {r2_train:.4f}")

# =========================================================
# 12. PLOT ACTUAL VS PREDICTED
# =========================================================

plt.figure(figsize=(6, 6))
plt.scatter(y_train, y_train_pred)
plt.xlabel("Actual Population")
plt.ylabel("Predicted Population")
plt.title("Actual vs Predicted (Training Data)")
plt.plot(
    [y_train.min(), y_train.max()],
    [y_train.min(), y_train.max()],
    'r--'
)
plt.tight_layout()
plt.show()

# =========================================================
# 13. SHOW TEST INPUT ROW
# =========================================================

print("-" * 50)
print("Test row features used for prediction:")
print(X_test)

print("-" * 50)
print("Done.")

# Save model and feature columns for later use in API
save_dir = r"C:\UnityProjects\FYP\GameWebAPI\app\ml\TrainedModels\Clustering"
os.makedirs(save_dir, exist_ok=True)

joblib.dump(regressor, os.path.join(save_dir, "lr_model.pkl"))
joblib.dump(feature_cols, os.path.join(save_dir, "lr_feature_cols.pkl"))
