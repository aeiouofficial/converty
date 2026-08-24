cmake_minimum_required(VERSION 3.28)
project(ConvertyNative LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

include(cmake/Hardening.cmake)

# B3 will add the Windows Explorer COM target. Keeping B0 target-free prevents
# a stub DLL from being mistaken for working shell integration.
add_custom_target(converty_native_topology ALL
    COMMAND ${CMAKE_COMMAND} -E echo "Converty native topology configured; Explorer target begins in B3."
    VERBATIM)
