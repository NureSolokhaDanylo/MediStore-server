-- ============================================================================
-- MediStore Test Data Generation Script
-- Current Date: 2026-03-30
-- ============================================================================

-- ============================================================================
-- INSERT MEDICINES (various types with different storage requirements)
-- ============================================================================
SET IDENTITY_INSERT dbo.Medicines ON;

INSERT INTO dbo.Medicines (Id, Name, Description, TempMax, TempMin, HumidMax, HumidMin, WarningThresholdDays)
VALUES
-- Room temperature medicines
(1, 'Aspirin 500mg', 'Analgesic and anti-inflammatory', 25.0, 15.0, 60.0, 30.0, 60),
(2, 'Ibuprofen 200mg', 'Pain relief and fever reducer', 25.0, 15.0, 60.0, 30.0, 60),
(3, 'Paracetamol 650mg', 'Fever and pain reliever', 25.0, 15.0, 65.0, 35.0, 60),
(4, 'Vitamin C 1000mg', 'Immune system support', 25.0, 15.0, 60.0, 30.0, 90),
(5, 'Calcium Tablets', 'Bone health supplement', 25.0, 16.0, 60.0, 35.0, 90),
-- Refrigerated medicines
(6, 'Insulin Vial 100IU/ml', 'Diabetes treatment', 8.0, 2.0, 50.0, 30.0, 30),
(7, 'Vaccine Batch A', 'Immunization vaccine', 8.0, 2.0, 45.0, 25.0, 45),
(8, 'Antibiotic Suspension', 'Bacterial infection treatment', 8.0, 2.0, 60.0, 30.0, 30),
-- Cool place medicines
(9, 'Ointment Cream', 'Skin treatment', 20.0, 15.0, 65.0, 40.0, 75),
(10, 'Cough Syrup', 'Cold and cough relief', 22.0, 15.0, 70.0, 35.0, 60);

SET IDENTITY_INSERT dbo.Medicines OFF;

-- ============================================================================
-- INSERT ZONES (storage areas)
-- ============================================================================
SET IDENTITY_INSERT dbo.Zones ON;

INSERT INTO dbo.Zones (Id, Name, Description, TempMax, TempMin, HumidMax, HumidMin)
VALUES
(1, 'Room Temp Zone A', 'Main storage for room temperature medicines', 25.0, 15.0, 65.0, 30.0),
(2, 'Room Temp Zone B', 'Secondary room temperature storage', 25.0, 15.0, 65.0, 30.0),
(3, 'Refrigerated Zone', 'Cold storage for vaccines and insulin', 8.0, 2.0, 50.0, 25.0),
(4, 'Cool Room Zone', 'Temperature controlled cool storage', 20.0, 15.0, 70.0, 40.0),
(5, 'Archive Zone', 'Archive for older batches', 25.0, 15.0, 65.0, 30.0);

SET IDENTITY_INSERT dbo.Zones OFF;

-- ============================================================================
-- INSERT SENSORS (2 per zone: 1 Temperature, 1 Humidity)
-- ============================================================================
SET IDENTITY_INSERT dbo.Sensors ON;

-- Zone 1 sensors
INSERT INTO dbo.Sensors (Id, SerialNumber, LastValue, LastUpdate, IsOn, SensorType, ZoneId)
VALUES
(1, 'TEMP-ZONE1-001', 22.5, '2026-03-30 14:30:00', 1, 1, 1),  -- Temperature sensor
(2, 'HUMID-ZONE1-001', 45.5, '2026-03-30 14:30:00', 1, 2, 1), -- Humidity sensor

-- Zone 2 sensors
(3, 'TEMP-ZONE2-001', 23.0, '2026-03-30 14:25:00', 1, 1, 2),
(4, 'HUMID-ZONE2-001', 48.0, '2026-03-30 14:25:00', 1, 2, 2),

-- Zone 3 (Refrigerated) sensors
(5, 'TEMP-ZONE3-001', 4.5, '2026-03-30 14:20:00', 1, 1, 3),
(6, 'HUMID-ZONE3-001', 35.0, '2026-03-30 14:20:00', 1, 2, 3),

-- Zone 4 (Cool Room) sensors
(7, 'TEMP-ZONE4-001', 18.5, '2026-03-30 14:15:00', 1, 1, 4),
(8, 'HUMID-ZONE4-001', 55.5, '2026-03-30 14:15:00', 1, 2, 4),

-- Zone 5 (Archive) sensors
(9, 'TEMP-ZONE5-001', 21.0, '2026-03-30 14:10:00', 1, 1, 5),
(10, 'HUMID-ZONE5-001', 50.0, '2026-03-30 14:10:00', 1, 2, 5);

SET IDENTITY_INSERT dbo.Sensors OFF;

-- ============================================================================
-- INSERT BATCHES
-- Expiration dates strategy for 2026-03-30:
-- - Already expired: 2026-01-15, 2026-02-28
-- - Expiring today: 2026-03-30
-- - Expiring very soon: 2026-04-05, 2026-04-15
-- - Normal future: 2026-05-30, 2026-06-30, 2026-09-30
-- ============================================================================
SET IDENTITY_INSERT dbo.Batches ON;

-- Medicine 1 (Aspirin) batches across zones
INSERT INTO dbo.Batches (Id, BatchNumber, Quantity, ExpireDate, DateAdded, MedicineId, ZoneId)
VALUES
(1, 'ASP-2025-001', 500, '2026-01-15 23:59:59', '2025-01-15 10:00:00', 1, 1),   -- EXPIRED
(2, 'ASP-2025-002', 300, '2026-02-28 23:59:59', '2025-02-01 10:00:00', 1, 1),   -- EXPIRED
(3, 'ASP-2026-001', 400, '2026-03-30 23:59:59', '2026-03-01 10:00:00', 1, 1),   -- EXPIRES TODAY
(4, 'ASP-2026-002', 350, '2026-04-05 23:59:59', '2026-03-15 10:00:00', 1, 2),   -- EXPIRES SOON
(5, 'ASP-2026-003', 600, '2026-06-30 23:59:59', '2026-03-20 10:00:00', 1, 2);  -- NORMAL

-- Medicine 2 (Ibuprofen) batches
INSERT INTO dbo.Batches (Id, BatchNumber, Quantity, ExpireDate, DateAdded, MedicineId, ZoneId)
VALUES
(6, 'IBU-2025-001', 450, '2026-02-10 23:59:59', '2025-02-10 10:00:00', 2, 1),   -- EXPIRED
(7, 'IBU-2026-001', 500, '2026-03-30 23:59:59', '2026-02-20 10:00:00', 2, 1),   -- EXPIRES TODAY
(8, 'IBU-2026-002', 400, '2026-04-15 23:59:59', '2026-03-10 10:00:00', 2, 2),   -- EXPIRES SOON
(9, 'IBU-2026-003', 550, '2026-07-15 23:59:59', '2026-03-22 10:00:00', 2, 2);  -- NORMAL

-- Medicine 3 (Paracetamol) batches
INSERT INTO dbo.Batches (Id, BatchNumber, Quantity, ExpireDate, DateAdded, MedicineId, ZoneId)
VALUES
(10, 'PAR-2025-001', 600, '2026-01-20 23:59:59', '2025-01-20 10:00:00', 3, 1),  -- EXPIRED
(11, 'PAR-2026-001', 500, '2026-03-30 23:59:59', '2026-03-05 10:00:00', 3, 1),  -- EXPIRES TODAY
(12, 'PAR-2026-002', 450, '2026-04-10 23:59:59', '2026-03-18 10:00:00', 3, 2),  -- EXPIRES SOON
(13, 'PAR-2026-003', 700, '2026-09-30 23:59:59', '2026-03-25 10:00:00', 3, 2);  -- NORMAL

-- Medicine 4 (Vitamin C) batches
INSERT INTO dbo.Batches (Id, BatchNumber, Quantity, ExpireDate, DateAdded, MedicineId, ZoneId)
VALUES
(14, 'VIT-C-2025-001', 800, '2026-02-28 23:59:59', '2025-03-01 10:00:00', 4, 1), -- EXPIRED
(15, 'VIT-C-2026-001', 700, '2026-03-30 23:59:59', '2026-02-15 10:00:00', 4, 1), -- EXPIRES TODAY
(16, 'VIT-C-2026-002', 600, '2026-05-01 23:59:59', '2026-03-20 10:00:00', 4, 5); -- NORMAL

-- Medicine 5 (Calcium) batches
INSERT INTO dbo.Batches (Id, BatchNumber, Quantity, ExpireDate, DateAdded, MedicineId, ZoneId)
VALUES
(17, 'CAL-2025-001', 500, '2026-01-31 23:59:59', '2025-02-01 10:00:00', 5, 1),  -- EXPIRED
(18, 'CAL-2026-001', 450, '2026-03-30 23:59:59', '2026-02-25 10:00:00', 5, 1),  -- EXPIRES TODAY
(19, 'CAL-2026-002', 600, '2026-08-30 23:59:59', '2026-03-25 10:00:00', 5, 5);  -- NORMAL

-- Medicine 6 (Insulin) refrigerated batches
INSERT INTO dbo.Batches (Id, BatchNumber, Quantity, ExpireDate, DateAdded, MedicineId, ZoneId)
VALUES
(20, 'INS-2026-001', 100, '2026-02-20 23:59:59', '2026-01-20 10:00:00', 6, 3),  -- EXPIRED
(21, 'INS-2026-002', 120, '2026-03-30 23:59:59', '2026-03-01 10:00:00', 6, 3),  -- EXPIRES TODAY
(22, 'INS-2026-003', 150, '2026-04-20 23:59:59', '2026-03-20 10:00:00', 6, 3),  -- EXPIRES SOON
(23, 'INS-2026-004', 110, '2026-06-15 23:59:59', '2026-03-25 10:00:00', 6, 3);  -- NORMAL

-- Medicine 7 (Vaccine) refrigerated batches
INSERT INTO dbo.Batches (Id, BatchNumber, Quantity, ExpireDate, DateAdded, MedicineId, ZoneId)
VALUES
(24, 'VAC-2026-001', 200, '2026-02-15 23:59:59', '2025-11-15 10:00:00', 7, 3),  -- EXPIRED
(25, 'VAC-2026-002', 250, '2026-03-30 23:59:59', '2026-01-20 10:00:00', 7, 3),  -- EXPIRES TODAY
(26, 'VAC-2026-003', 300, '2026-04-25 23:59:59', '2026-03-10 10:00:00', 7, 3),  -- EXPIRES SOON
(27, 'VAC-2026-004', 275, '2026-07-30 23:59:59', '2026-03-22 10:00:00', 7, 3);  -- NORMAL

-- Medicine 8 (Antibiotic) refrigerated batches
INSERT INTO dbo.Batches (Id, BatchNumber, Quantity, ExpireDate, DateAdded, MedicineId, ZoneId)
VALUES
(28, 'ANT-2026-001', 150, '2026-01-25 23:59:59', '2025-12-01 10:00:00', 8, 3),  -- EXPIRED
(29, 'ANT-2026-002', 180, '2026-03-30 23:59:59', '2026-02-28 10:00:00', 8, 3),  -- EXPIRES TODAY
(30, 'ANT-2026-003', 160, '2026-04-12 23:59:59', '2026-03-15 10:00:00', 8, 3),  -- EXPIRES SOON
(31, 'ANT-2026-004', 200, '2026-06-20 23:59:59', '2026-03-24 10:00:00', 8, 3);  -- NORMAL

-- Medicine 9 (Ointment) cool place batches
INSERT INTO dbo.Batches (Id, BatchNumber, Quantity, ExpireDate, DateAdded, MedicineId, ZoneId)
VALUES
(32, 'OTN-2025-001', 300, '2026-03-01 23:59:59', '2025-04-01 10:00:00', 9, 4),  -- EXPIRED
(33, 'OTN-2026-001', 280, '2026-03-30 23:59:59', '2026-02-20 10:00:00', 9, 4),  -- EXPIRES TODAY
(34, 'OTN-2026-002', 320, '2026-05-15 23:59:59', '2026-03-20 10:00:00', 9, 4);  -- NORMAL

-- Medicine 10 (Cough Syrup) batches
INSERT INTO dbo.Batches (Id, BatchNumber, Quantity, ExpireDate, DateAdded, MedicineId, ZoneId)
VALUES
(35, 'COUGH-2025-001', 400, '2026-02-05 23:59:59', '2025-03-05 10:00:00', 10, 4), -- EXPIRED
(36, 'COUGH-2026-001', 350, '2026-03-30 23:59:59', '2026-03-01 10:00:00', 10, 4), -- EXPIRES TODAY
(37, 'COUGH-2026-002', 380, '2026-04-20 23:59:59', '2026-03-18 10:00:00', 10, 4), -- EXPIRES SOON
(38, 'COUGH-2026-003', 420, '2026-08-15 23:59:59', '2026-03-25 10:00:00', 10, 4); -- NORMAL

SET IDENTITY_INSERT dbo.Batches OFF;

-- ============================================================================
-- Summary Statistics
-- ============================================================================
PRINT '============================================================================';
PRINT 'INSERT Test Data Generation Complete';
PRINT '============================================================================';
PRINT '';
PRINT 'Inserted:';
PRINT '  - 10 Medicines';
PRINT '  - 5 Zones with 10 Sensors (2 per zone: 1 Temperature + 1 Humidity)';
PRINT '  - 38 Batches with various expiration dates';
PRINT '';
PRINT 'Expiration Date Distribution (as of 2026-03-30):';
SELECT 
    CASE 
        WHEN ExpireDate < '2026-03-30' THEN 'EXPIRED'
        WHEN ExpireDate = '2026-03-30' THEN 'EXPIRES TODAY'
        WHEN ExpireDate BETWEEN '2026-03-31' AND '2026-04-20' THEN 'EXPIRES SOON (0-3 weeks)'
        ELSE 'NORMAL (>3 weeks)'
    END AS Status,
    COUNT(*) AS Count
FROM dbo.Batches
GROUP BY
    CASE 
        WHEN ExpireDate < '2026-03-30' THEN 'EXPIRED'
        WHEN ExpireDate = '2026-03-30' THEN 'EXPIRES TODAY'
        WHEN ExpireDate BETWEEN '2026-03-31' AND '2026-04-20' THEN 'EXPIRES SOON (0-3 weeks)'
        ELSE 'NORMAL (>3 weeks)'
    END
ORDER BY Status;

PRINT '';
PRINT 'Sensor Distribution per Zone:';
SELECT Z.Name AS Zone, COUNT(S.Id) AS SensorCount
FROM dbo.Zones Z
LEFT JOIN dbo.Sensors S ON Z.Id = S.ZoneId
GROUP BY Z.Id, Z.Name
ORDER BY Z.Id;

PRINT '';
PRINT 'Test data is ready for use!';
PRINT '============================================================================';
