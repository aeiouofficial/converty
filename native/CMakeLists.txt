cmake_minimum_required(VERSION 3.28)
project(ConvertyNative LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

include(cmake/Hardening.cmake)

if(WIN32)
    add_library(Converty.ShellExtension SHARED
        Converty.ShellExtension/ConvertyShellExtension.cpp
        Converty.ShellExtension/Converty.ShellExtension.def)

    target_compile_definitions(Converty.ShellExtension PRIVATE
        UNICODE
        _UNICODE)

    target_link_libraries(Converty.ShellExtension PRIVATE
        ole32
        shell32
        user32
        uuid)

    converty_apply_msvc_hardening(Converty.ShellExtension)

    set_target_properties(Converty.ShellExtension PROPERTIES
        PREFIX ""
        OUTPUT_NAME "Converty.ShellExtension")

    add_executable(Converty.ExplorerRegistrationSmoke
        Converty.ShellExtension/ExplorerRegistrationSmoke.cpp)

    target_compile_definitions(Converty.ExplorerRegistrationSmoke PRIVATE
        UNICODE
        _UNICODE)

    target_link_libraries(Converty.ExplorerRegistrationSmoke PRIVATE
        onecore
        ole32
        shell32
        uuid)

    converty_apply_msvc_hardening(Converty.ExplorerRegistrationSmoke)
else()
    add_custom_target(converty_native_topology ALL
        COMMAND ${CMAKE_COMMAND} -E echo "Converty Explorer shell extension is Windows-only."
        VERBATIM)
endif()
