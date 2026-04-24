import pandas as pd
import numpy as np
from pathlib import Path
from GameWebAPI.app.ml.Analytics.preprocessing.PreprocessingExporter import export_session_features

# ====== SET YOUR FILE PATHS HERE ======
action_log_path = Path(r"C:\UnityProjects\FYP\Analytics\Logs\action_logs_20260401_191909.csv")
turn_snapshot_path = Path(r"C:\UnityProjects\FYP\Analytics\Logs\turn_snapshots_20260401_191909.csv")

pd.set_option('display.max_columns', None)
pd.set_option('display.width', None)
pd.set_option('display.max_colwidth', None)

# ====== READ CSV INTO PANDAS ======
action_df = pd.read_csv(action_log_path)
turn_df = pd.read_csv(turn_snapshot_path)

print("=== Action Logs DataFrame ===")
print(action_df.head())
print()
print("=== Turn Snapshots DataFrame ===")
print(turn_df.head())
print()

# ====== OPTIONAL: CHECK COLUMNS ======
print("Action log columns:")
print(action_df.columns.tolist())
print()
print("Turn snapshot columns:")
print(turn_df.columns.tolist())
print()

# ====== CONVERT SELECTED NUMERIC COLUMNS TO NUMPY ======
action_numeric_cols = [
    "Turn",
    "TimeSinceSessionStart",
    "PositionX",
    "PositionY",
    "TargetBuildingLevelBefore",
    "TargetBuildingLevelAfter",
    "GoldBefore",
    "GoldAfter",
    "APBefore",
    "APAfter",
    "WasValid"
]

turn_numeric_cols = [
    "Turn",
    "Gold",
    "Population",
    "TotalSupplyProvided",
    "AP",
    "APUsed",
    "UpgradeCount",
    "DemolishCount",
    "SmallHouseCount",
    "BigHouseCount",
    "SupplyCount",
    "ServiceCount",
    "FactoryCount",
    "RoadCount",
    "AverageSatisfactionIndex",
    "MinSatisfactionIndex",
    "MaxSatisfactionIndex",
    "AveragePollutionIndex",
    "MinPollutionIndex",
    "MaxPollutionIndex",
    "AverageServiceIndex",
    "MinServiceIndex",
    "MaxServiceIndex",
    "HousesNearFactoryCount",
    "HousesWithoutServiceCount",
    "HousesLowSatisfactionCount",
    "TotalTaxIncome"
]

action_np = action_df[action_numeric_cols].to_numpy()
turn_np = turn_df[turn_numeric_cols].to_numpy()

print("=== Action Logs NumPy Array ===")
print(action_np)
print("Shape:", action_np.shape)
print()

print("=== Turn Snapshots NumPy Array ===")
print(turn_np)
print("Shape:", turn_np.shape)
print()

# ====== OPTIONAL: SEPARATE FEATURES ONLY ======
# Good for ML use later
X_turn = turn_df[turn_numeric_cols].astype(float).to_numpy()

print("=== Turn Feature Matrix for ML ===")
print(X_turn)
print("Shape:", X_turn.shape)

# preprocessing starts
preprocessed_turn_df = turn_df.copy()

# player actions
# buildings
preprocessed_turn_df["BuiltHouseCount"] = preprocessed_turn_df["SmallHouseCount"].diff() + preprocessed_turn_df["BigHouseCount"].diff()
preprocessed_turn_df["BuiltFactoryCount"] = preprocessed_turn_df["FactoryCount"].diff()
preprocessed_turn_df["BuiltRoadCount"] = preprocessed_turn_df["RoadCount"].diff()
preprocessed_turn_df["BuiltServiceCount"] = preprocessed_turn_df["ServiceCount"].diff()
preprocessed_turn_df["BuiltSupplyCount"] = preprocessed_turn_df["SupplyCount"].diff()
preprocessed_turn_df["RoadCount"] = preprocessed_turn_df["RoadCount"].diff()

preprocessed_turn_df["TotalBuildCount"] = preprocessed_turn_df["BuiltHouseCount"] + preprocessed_turn_df["BuiltFactoryCount"] + preprocessed_turn_df["BuiltRoadCount"] + preprocessed_turn_df["BuiltServiceCount"] + preprocessed_turn_df["BuiltSupplyCount"]

preprocessed_turn_df["TurnActionCount"] = preprocessed_turn_df["UpgradeCount"] + preprocessed_turn_df["DemolishCount"] + preprocessed_turn_df["BuiltHouseCount"] + preprocessed_turn_df["BuiltFactoryCount"] + preprocessed_turn_df["BuiltRoadCount"] + preprocessed_turn_df["BuiltServiceCount"] + preprocessed_turn_df["BuiltSupplyCount"]

preprocessed_turn_df["ActionCount"] = preprocessed_turn_df["UpgradeCount"] + preprocessed_turn_df["DemolishCount"] 
preprocessed_turn_df["ActionIntensity"] = preprocessed_turn_df["APUsed"] / preprocessed_turn_df["AP"].replace(0, np.nan)
preprocessed_turn_df["UpgradeRatio"] = preprocessed_turn_df["UpgradeCount"] / preprocessed_turn_df["TurnActionCount"].replace(0, np.nan)
preprocessed_turn_df["DemolishRatio"] = preprocessed_turn_df["DemolishCount"] / preprocessed_turn_df["TurnActionCount"].replace(0, np.nan)
preprocessed_turn_df["BuildRatio"] = preprocessed_turn_df["TotalBuildCount"] / preprocessed_turn_df["TurnActionCount"].replace(0, np.nan)

preprocessed_turn_df["BuildHouseRatio"] = preprocessed_turn_df["BuiltHouseCount"] / preprocessed_turn_df["TotalBuildCount"].replace(0, np.nan)
preprocessed_turn_df["BuildFactoryRatio"] = preprocessed_turn_df["BuiltFactoryCount"] / preprocessed_turn_df["TotalBuildCount"].replace(0, np.nan)
preprocessed_turn_df["BuildServiceRatio"] = preprocessed_turn_df["BuiltServiceCount"] / preprocessed_turn_df["TotalBuildCount"].replace(0, np.nan)
preprocessed_turn_df["BuildSupplyRatio"] = preprocessed_turn_df["BuiltSupplyCount"] / preprocessed_turn_df["TotalBuildCount"].replace(0, np.nan)

preprocessed_turn_df["HousePerRoad"] = preprocessed_turn_df["BuiltHouseCount"] / preprocessed_turn_df["BuiltRoadCount"].replace(0, np.nan)
preprocessed_turn_df["PopulationPerAP"] = preprocessed_turn_df["Population"] / (preprocessed_turn_df["APUsed"]).replace(0, np.nan)
preprocessed_turn_df["GoldPerPopulation"] = preprocessed_turn_df["Gold"] / preprocessed_turn_df["Population"].replace(0, np.nan)
preprocessed_turn_df["TaxIncomePerPopulation"] = preprocessed_turn_df["TotalTaxIncome"] / preprocessed_turn_df["Population"].replace(0, np.nan)

# risk/mistakes
# preprocessed_turn_df['BadPlacementRate'] = preprocessed_turn_df['HousesNearFactoryCount'] / (preprocessed_turn_df['BuiltHouseCount']).replace(0, np.nan)
# preprocessed_turn_df['NoServiceRate'] = preprocessed_turn_df['HousesWithoutServiceCount'] / (preprocessed_turn_df['BuiltHouseCount']).replace(0, np.nan)
# preprocessed_turn_df['LowSatisfactionRate'] = preprocessed_turn_df['HousesLowSatisfactionCount'] / (preprocessed_turn_df['BuiltHouseCount']).replace(0, np.nan)

total_houses = preprocessed_turn_df["SmallHouseCount"] + preprocessed_turn_df["BigHouseCount"]

preprocessed_turn_df["BadPlacementRate"] = preprocessed_turn_df["HousesNearFactoryCount"] / total_houses.replace(0, np.nan)
preprocessed_turn_df["NoServiceRate"] = preprocessed_turn_df["HousesWithoutServiceCount"] / total_houses.replace(0, np.nan)
preprocessed_turn_df["LowSatisfactionRate"] = preprocessed_turn_df["HousesLowSatisfactionCount"] / total_houses.replace(0, np.nan)

preprocessed_turn_df.fillna(0, inplace=True)
# averages
# avg_turn_action_count = preprocessed_turn_df["TurnActionCount"].mean()
avg_action_intensity = preprocessed_turn_df["ActionIntensity"].mean()
std_action_intensity = preprocessed_turn_df["ActionIntensity"].std()
avg_upgrade_ratio = preprocessed_turn_df["UpgradeRatio"].mean()
avg_demolish_ratio = preprocessed_turn_df["DemolishRatio"].mean()
# avg_build_ratio = preprocessed_turn_df["BuildRatio"].mean()

avg_build_house_ratio = preprocessed_turn_df["BuildHouseRatio"].mean()
avg_build_factory_ratio = preprocessed_turn_df["BuildFactoryRatio"].mean()
avg_build_service_ratio = preprocessed_turn_df["BuildServiceRatio"].mean()
avg_build_supply_ratio = preprocessed_turn_df["BuildSupplyRatio"].mean()

avg_gold_per_population = preprocessed_turn_df["GoldPerPopulation"].mean()
# avg_tax_per_population = preprocessed_turn_df["TaxIncomePerPopulation"].mean()

avg_bad_placement = preprocessed_turn_df["BadPlacementRate"].mean()
avg_no_service = preprocessed_turn_df["NoServiceRate"].mean()
# avg_low_satisfaction = preprocessed_turn_df["LowSatisfactionRate"].mean()

# total_builds = preprocessed_turn_df["TotalBuildCount"].sum()
# total_actions = preprocessed_turn_df["TurnActionCount"].sum()

# index growths
preprocessed_turn_df["PopulationGrowth"] = preprocessed_turn_df["Population"].diff()
preprocessed_turn_df["GoldGrowth"] = preprocessed_turn_df["Gold"].diff()
preprocessed_turn_df["APGrowth"] = preprocessed_turn_df["AP"].diff()
preprocessed_turn_df["SupplyGrowth"] = preprocessed_turn_df["TotalSupplyProvided"].diff()

avg_population_growth = preprocessed_turn_df["PopulationGrowth"].mean()
avg_satisfaction_index = preprocessed_turn_df["AverageSatisfactionIndex"].mean()
# avg_satisfaction_growth = preprocessed_turn_df["AverageSatisfactionIndex"].diff().mean()
# avg_pollution_growth = preprocessed_turn_df["AveragePollutionIndex"].diff().mean()
avg_service_growth = preprocessed_turn_df["AverageServiceIndex"].diff().mean()
avg_gold_growth = preprocessed_turn_df["GoldGrowth"].mean()
avg_supply_growth = preprocessed_turn_df["SupplyGrowth"].mean()

std_gold_growth = preprocessed_turn_df["GoldGrowth"].std()
std_population_growth = preprocessed_turn_df["PopulationGrowth"].std()
std_satisfaction_growth = preprocessed_turn_df["AverageSatisfactionIndex"].diff().std()
# std_pollution_growth = preprocessed_turn_df["AveragePollutionIndex"].diff().std()
# std_service_growth = preprocessed_turn_df["AverageServiceIndex"].diff().std()

total_population_growth = preprocessed_turn_df['Population'].iloc[-1] - preprocessed_turn_df['Population'].iloc[0]
total_gold_growth = preprocessed_turn_df['Gold'].iloc[-1] - preprocessed_turn_df['Gold'].iloc[0]
total_supply_growth = preprocessed_turn_df['TotalSupplyProvided'].iloc[-1] - preprocessed_turn_df['TotalSupplyProvided'].iloc[0]
total_ap_growth = preprocessed_turn_df['AP'].iloc[-1] - preprocessed_turn_df['AP'].iloc[0]
supply_ratio = preprocessed_turn_df['TotalSupplyProvided'] / preprocessed_turn_df['Population'].replace(0, np.nan)

supply_efficiency = total_supply_growth / total_population_growth
avg_house_per_turn = preprocessed_turn_df["BuiltHouseCount"].mean()
avg_factory_per_turn = preprocessed_turn_df["BuiltFactoryCount"].mean()


session_features = {
    "session_id": "20260331_051053",
    "avg_action_intensity": avg_action_intensity,
    "std_action_intensity": std_action_intensity,
    "avg_upgrade_ratio": avg_upgrade_ratio,
    "avg_demolish_ratio": avg_demolish_ratio,
    "avg_build_house_ratio": avg_build_house_ratio,
    "avg_build_factory_ratio": avg_build_factory_ratio,
    "avg_build_service_ratio": avg_build_service_ratio,
    "avg_build_supply_ratio": avg_build_supply_ratio,
    "avg_gold_per_population": avg_gold_per_population,
    "avg_bad_placement": avg_bad_placement,
    "avg_no_service": avg_no_service,
    # "total_builds": total_builds,
    # "total_actions": total_actions,
    "avg_population_growth": avg_population_growth,
    "avg_satisfaction_index": avg_satisfaction_index,
    "avg_service_growth": avg_service_growth,
    "avg_gold_growth": avg_gold_growth,
    "avg_supply_growth": avg_supply_growth,
    "std_gold_growth": std_gold_growth,
    "std_population_growth": std_population_growth,
    "std_satisfaction_growth": std_satisfaction_growth,
    "total_population_growth": total_population_growth,
    "total_gold_growth": total_gold_growth,
    "total_supply_growth": total_supply_growth,
    "total_ap_growth": total_ap_growth,
    "supply_efficiency": supply_efficiency,
    "avg_house_per_turn": avg_house_per_turn,
    "avg_factory_per_turn": avg_factory_per_turn
}

session_features_df = pd.DataFrame([session_features])
print("=== Session-Level Features ===")
for col in session_features_df.columns:
    print(f"\n===== COLUMN: {col} =====")
    print(session_features_df[col].head(10))  # change to .unique() if categorical

export_session_features(session_features)

# numeric_df = session_features_df.drop(columns=["session_id"], errors="ignore").select_dtypes(include=[np.number])

# print("Rows:", len(numeric_df))

# if len(numeric_df) < 2:
#     print("Correlation cannot be computed meaningfully with fewer than 2 rows.")
# else:
#     print(numeric_df.corr())