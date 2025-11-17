-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Lis 03, 2025 at 07:57 AM
-- Wersja serwera: 10.4.32-MariaDB
-- Wersja PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `inpost`
--

-- --------------------------------------------------------

--
-- Struktura tabeli dla tabeli `admins`
--

CREATE TABLE `admins` (
  `admin_id` int(11) NOT NULL,
  `user_id` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_polish_ci;

-- --------------------------------------------------------

--
-- Struktura tabeli dla tabeli `boxs`
--

CREATE TABLE `boxs` (
  `box_id` int(11) NOT NULL,
  `pack_id` int(11) DEFAULT NULL,
  `size` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_polish_ci;

-- --------------------------------------------------------

--
-- Struktura tabeli dla tabeli `klients`
--

CREATE TABLE `klients` (
  `klient_id` int(11) NOT NULL,
  `user_id` int(11) DEFAULT NULL,
  `pack_id` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_polish_ci;

-- --------------------------------------------------------

--
-- Struktura tabeli dla tabeli `kuriers`
--

CREATE TABLE `kuriers` (
  `kurier_id` int(11) NOT NULL,
  `user_id` int(11) DEFAULT NULL,
  `state` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_polish_ci;

-- --------------------------------------------------------

--
-- Struktura tabeli dla tabeli `kuriers_data`
--

CREATE TABLE `kuriers_data` (
  `kurier_id` int(11) DEFAULT NULL,
  `pack_id` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_polish_ci;

-- --------------------------------------------------------

--
-- Struktura tabeli dla tabeli `packs`
--

CREATE TABLE `packs` (
  `pack_id` int(11) NOT NULL,
  `size` text DEFAULT NULL,
  `klient_id` int(11) DEFAULT NULL,
  `delivered` tinyint(1) DEFAULT NULL,
  `date` date DEFAULT NULL,
  `to_when` date DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_polish_ci;

-- --------------------------------------------------------

--
-- Struktura tabeli dla tabeli `paczkomat`
--

CREATE TABLE `paczkomat` (
  `paczkomat_id` int(11) NOT NULL,
  `paczkomat_name` text DEFAULT NULL,
  `address` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_polish_ci;

-- --------------------------------------------------------

--
-- Struktura tabeli dla tabeli `paczkomat_data`
--

CREATE TABLE `paczkomat_data` (
  `paczkomat_id` int(11) DEFAULT NULL,
  `box_id` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_polish_ci;

-- --------------------------------------------------------

--
-- Struktura tabeli dla tabeli `users`
--

CREATE TABLE `users` (
  `id` int(11) NOT NULL,
  `name` text DEFAULT NULL,
  `surname` text DEFAULT NULL,
  `email` text DEFAULT NULL,
  `date_of_birth` date DEFAULT NULL,
  `password` text DEFAULT NULL,
  `role` text DEFAULT NULL,
  `phone_number` int(11) DEFAULT NULL,
  `selfie` text DEFAULT NULL,
  `sex` tinyint(1) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_polish_ci;

--
-- Indeksy dla zrzutów tabel
--

--
-- Indeksy dla tabeli `admins`
--
ALTER TABLE `admins`
  ADD PRIMARY KEY (`admin_id`),
  ADD KEY `user_id` (`user_id`);

--
-- Indeksy dla tabeli `boxs`
--
ALTER TABLE `boxs`
  ADD PRIMARY KEY (`box_id`),
  ADD KEY `pack_id` (`pack_id`);

--
-- Indeksy dla tabeli `klients`
--
ALTER TABLE `klients`
  ADD PRIMARY KEY (`klient_id`),
  ADD KEY `user_id` (`user_id`,`pack_id`);

--
-- Indeksy dla tabeli `kuriers`
--
ALTER TABLE `kuriers`
  ADD PRIMARY KEY (`kurier_id`),
  ADD KEY `user_id` (`user_id`);

--
-- Indeksy dla tabeli `kuriers_data`
--
ALTER TABLE `kuriers_data`
  ADD KEY `kurier_id` (`kurier_id`,`pack_id`),
  ADD KEY `pack_id` (`pack_id`);

--
-- Indeksy dla tabeli `packs`
--
ALTER TABLE `packs`
  ADD PRIMARY KEY (`pack_id`),
  ADD KEY `klient_id` (`klient_id`);

--
-- Indeksy dla tabeli `paczkomat`
--
ALTER TABLE `paczkomat`
  ADD PRIMARY KEY (`paczkomat_id`);

--
-- Indeksy dla tabeli `paczkomat_data`
--
ALTER TABLE `paczkomat_data`
  ADD KEY `paczkomat_id` (`paczkomat_id`,`box_id`),
  ADD KEY `box_id` (`box_id`);

--
-- Indeksy dla tabeli `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`id`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `admins`
--
ALTER TABLE `admins`
  MODIFY `admin_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT for table `boxs`
--
ALTER TABLE `boxs`
  MODIFY `box_id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `klients`
--
ALTER TABLE `klients`
  MODIFY `klient_id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `kuriers`
--
ALTER TABLE `kuriers`
  MODIFY `kurier_id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `packs`
--
ALTER TABLE `packs`
  MODIFY `pack_id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `paczkomat`
--
ALTER TABLE `paczkomat`
  MODIFY `paczkomat_id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `users`
--
ALTER TABLE `users`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `admins`
--
ALTER TABLE `admins`
  ADD CONSTRAINT `admins_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `boxs`
--
ALTER TABLE `boxs`
  ADD CONSTRAINT `boxs_ibfk_1` FOREIGN KEY (`pack_id`) REFERENCES `packs` (`pack_id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `klients`
--
ALTER TABLE `klients`
  ADD CONSTRAINT `klients_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `kuriers`
--
ALTER TABLE `kuriers`
  ADD CONSTRAINT `kuriers_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `kuriers_data`
--
ALTER TABLE `kuriers_data`
  ADD CONSTRAINT `kuriers_data_ibfk_1` FOREIGN KEY (`pack_id`) REFERENCES `packs` (`pack_id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `kuriers_data_ibfk_2` FOREIGN KEY (`kurier_id`) REFERENCES `kuriers` (`kurier_id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `packs`
--
ALTER TABLE `packs`
  ADD CONSTRAINT `packs_ibfk_1` FOREIGN KEY (`klient_id`) REFERENCES `klients` (`klient_id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `paczkomat_data`
--
ALTER TABLE `paczkomat_data`
  ADD CONSTRAINT `paczkomat_data_ibfk_1` FOREIGN KEY (`paczkomat_id`) REFERENCES `paczkomat` (`paczkomat_id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `paczkomat_data_ibfk_2` FOREIGN KEY (`box_id`) REFERENCES `boxs` (`box_id`) ON DELETE CASCADE ON UPDATE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
